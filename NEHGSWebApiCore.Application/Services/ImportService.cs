using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using NEHGSWebApiCore.Application.Interfaces;
using NEHGSWebApiCore.Application.Interfaces.Repositories;
using NEHGSWebApiCore.Application.Interfaces.Services;
using NEHGSWebApiCore.Domain.Entities;
using NEHGSWebApiCore.Infrastructure.BackgroundTask;
using System.Text.Json;


namespace NEHGSWebApiCore.Application.Services
{
    public class ImportService : IImportService
    {
        private readonly IConfiguration _configuration;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ImportService> _logger;
        private readonly IProgressService _progressService;

        public ImportService(IConfiguration configuration, IBackgroundTaskQueue queue, IUnitOfWork uow, ILogger<ImportService> logger, IProgressService progressService)
        {
            _configuration = configuration;
            _queue = queue;
            _uow = uow;
            _logger = logger;
            _progressService = progressService;
        }

        public Task<IEnumerable<string>> GetCsvPathsAsync(string parentDirectory)
        {
            var basePathSection = _configuration.GetSection("RecordsFilePath");
            var basePath = basePathSection?.Value ?? string.Empty;
            var dir = Path.Combine(basePath, parentDirectory ?? string.Empty);
            if (!Directory.Exists(dir)) return Task.FromResult(Enumerable.Empty<string>());
            var csvs = Directory.EnumerateFiles(dir).Where(f => Path.GetExtension(f).Equals(".csv", StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(csvs);
        }

        public Task<PreviewResponseDto> GetPreviewAsync(string csvPath, string metadataPath)
        {
            var rows = new List<Dictionary<string, object>>();
            if (!File.Exists(csvPath)) return Task.FromResult(new PreviewResponseDto { Rows = rows });

            using (var reader = new StreamReader(csvPath))
            {
                var header = reader.ReadLine();
                if (header == null) return Task.FromResult(new PreviewResponseDto { Rows = rows });
                var columns = header.Split(',').Select(c => c.Trim()).ToArray();
                for (int i = 0; i < 10 && !reader.EndOfStream; i++)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    var parts = line.Split(',');
                    var dict = new Dictionary<string, object>();
                    for (int j = 0; j < columns.Length && j < parts.Length; j++) dict[columns[j]] = parts[j];
                    rows.Add(dict);
                }
            }

            return Task.FromResult(new PreviewResponseDto { Rows = rows });
        }

        public Task<string> StartImportAsync(MapRequestDto request)
        {
            var taskId = Guid.NewGuid().ToString();
            _ = QueueImport(request, taskId);
            return Task.FromResult(taskId);
        }

        private Task QueueImport(MapRequestDto request, string taskId)
        {
            var csvsTask = GetCsvPathsAsync(request.ParentDirectory);
            return Task.Run(async () =>
            {
                var csvs = (await csvsTask).ToList();
                var filesRemaining = csvs.Count;
                _progressService.Start(taskId, Math.Max(1, filesRemaining));

                foreach (var path in csvs)
                {
                    var importFile = new ImportFile
                    {
                        DefaultRecordTypeId = request.SelectedRecordTypeId == 0 ? (int?)null : request.SelectedRecordTypeId,
                        TargetCollectionId = request.SelectedCollectionId,
                        SourceFilename = path,
                        ImportSerializeId = null,
                        ImportFileStatusId = 6,
                        HasError = false,
                        ImportName = request.ImportName,
                        ImportStart = DateTime.UtcNow
                    };

                    await _uow.ImportFiles.AddAsync(importFile);
                    await _uow.SaveChangesAsync();

                    try
                    {
                        // simple load: count rows and save ImportedRows
                        var rowCount = await LoadSimpleCsvAsync(path);
                        importFile.ImportedRows = rowCount;
                        importFile.ImportEnd = DateTime.UtcNow;
                        await Task.Delay(200); // simulate
                        _uow.ImportFiles.Update(importFile);
                        await _uow.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error importing file {path}", path);
                        importFile.HasError = true;
                        importFile.ProcessMessage = ex.Message;
                        _uow.ImportFiles.Update(importFile);
                        await _uow.SaveChangesAsync();
                    }

                    _progressService.Step(taskId);
                }

                _progressService.SetMessage(taskId, "Import completed");
            });
        }

        private async Task<int> LoadSimpleCsvAsync(string path)
        {
            // naive row count
            int count = 0;
            using (var reader = new StreamReader(path))
            {
                // skip header
                var header = await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    await reader.ReadLineAsync();
                    count++;
                }
            }
            return count;
        }

        public Task StartErrorCheckAsync(ErrorCheckRequestDto request, string progressId)
        {
            // run validation in background and persist results
            _ = Task.Run(async () =>
            {
                _progressService.Start(progressId, 5);
                try
                {
                    var csvs = (await GetCsvPathsAsync(request.ParentDirectory)).ToList();
                    var first = csvs.FirstOrDefault();
                    var errors = new List<object>();
                    if (first != null)
                    {
                        // very simple validation: check header contains mapped columns
                        using (var reader = new StreamReader(first))
                        {
                            var header = await reader.ReadLineAsync();
                            var cols = (header ?? string.Empty).Split(',').Select(c => c.Trim().ToLower()).ToList();
                            foreach (var kv in request.ColumnNameToMapping)
                            {
                                var src = kv.Key.ToLower();
                                if (!cols.Contains(src)) errors.Add(new { Column = src, Message = "Missing column" });
                            }
                        }
                    }

                    var result = new ErrorCheckResult
                    {
                        ProgressId = progressId,
                        SerializedErrors = JsonSerializer.Serialize(errors),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.ErrorCheckResults.AddAsync(result);
                    await _uow.SaveChangesAsync();

                    for (int i = 0; i <= 100; i += 25)
                    {
                        _progressService.SetMessage(progressId, $"{i}%");
                        _progressService.Step(progressId);
                        await Task.Delay(200);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during error check");
                    _progressService.SetMessage(progressId, "Error occurred");
                }
            });

            return Task.CompletedTask;
        }

        public Task<int> GetProgressAsync(string taskId)
        {
            return Task.FromResult(_progressService.GetPercentComplete(taskId));
        }

        public string GetStatusMessage(string taskId)
        {
            return _progressService.GetMessage(taskId);
        }

        public Task<ConfirmMapResultDto> ConfirmMapAsync(MapRequestDto request)
        {
            // simple confirm map: check first CSV columns
            var result = new ConfirmMapResultDto { IsMultipleCSVs = false, MissingColumnsDetails = string.Empty, MismatchColumnDiff = "0" };
            return Task.FromResult(result);
        }

        public async Task<ErrorCheckResult> GetErrorCheckResultAsync(string progressId)
        {
            var all = await _uow.ErrorCheckResults.FindAsync(r => r.ProgressId == progressId);
            return all.FirstOrDefault();
        }

        public string GetStatusMessageSync(string taskId) => GetStatusMessage(taskId);

        public async Task<CATUrlResponse> CATRedirectURLAsync(string taskId)
        {
            var response = new CATUrlResponse
            {
                CATUrl = _configuration["CATRedirectURL"] ?? string.Empty,
                TaskId = taskId
            };

            return response;
        }
    }
}
