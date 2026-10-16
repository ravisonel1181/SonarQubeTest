using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using NEHGSWebApiCore.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;

        public FileStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ApiResponse> SaveCsvAsync(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var ext = System.IO.Path.GetExtension(file.FileName);
            if (!string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse.Fail("Only CSV files are allowed.", System.Net.HttpStatusCode.MethodNotAllowed);
            }

            var size = file.Length;
            var formatted = ConvertBytesToMBFormatted(size);

            var configuredName = _configuration["CsvUploadFileName"];
            var fileName = string.IsNullOrEmpty(configuredName) ? file.FileName : configuredName;

            var folder = _configuration["CsvFileUploadPath"];
            if (string.IsNullOrEmpty(folder))
            {
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NEHGS", "Uploads");
            }

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var safeFileName = string.IsNullOrEmpty(fileName) ? file.FileName : fileName;
            var dest = Path.Combine(folder, safeFileName);

            // stream to disk
            using (var stream = new FileStream(dest, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }           

            var result = new UploadResultDto { Path = dest, Size = size, SizeFormatted = formatted };

            return ApiResponse.Success(result, "File uploaded successfully.");
        }

        private string ConvertBytesToMBFormatted(long bytes)
        {
            double megabytes = bytes / (1024.0 * 1024.0);
            return megabytes.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}
