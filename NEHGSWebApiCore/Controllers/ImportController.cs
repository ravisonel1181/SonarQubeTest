using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NEHGSWebApiCore.Application.Common;
using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using NEHGSWebApiCore.Application.Interfaces;
using NEHGSWebApiCore.Application.Interfaces.Repositories;
using NEHGSWebApiCore.Application.Interfaces.Services;
using NEHGSWebApiCore.Infrastructure.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImportController> _logger;

        public ImportController(IImportService importService, IUnitOfWork unitOfWork, ILogger<ImportController> logger)
        {
            _importService = importService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            // return collections and available gedcom parents (simple)
            var collections = await _unitOfWork.Collections.GetAllAsync();
            // Gedcom wrappers discovery not implemented in API; return empty
            var response = new { Collections = collections, GedcomFilesWrappers = Array.Empty<object>() };
            return Ok(ApiResponse<object>.Ok(response));
        }

        [HttpGet("map")]
        public async Task<IActionResult> Map([FromQuery] string parentDirectory, [FromQuery] bool csvOnly = false, [FromQuery] int collectionid = 0, [FromQuery] string csvmapping = "")
        {
            var progressId = Guid.NewGuid().ToString();

            try
            {
                var csvs = await _importService.GetCsvPathsAsync(parentDirectory);
                var hasCsvs = csvs != null && csvs.Any();
                string csvPreviewPath = hasCsvs ? csvs.First() : null;

                PreviewResponseDto preview = null;
                if (hasCsvs && !string.IsNullOrEmpty(csvPreviewPath))
                {
                    preview = await _importService.GetPreviewAsync(csvPreviewPath, null);
                }

                var serialized = await _unitOfWork.ImportSerializedMappings.GetAllAsync();
                var serializedNames = serialized.Select(s => s.MappingName).ToList();

                var result = new
                {
                    Preview = preview,
                    SerializedMappings = serializedNames,
                    ProgressId = progressId
                };

                return Ok(ApiResponse<object>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Map GET failed");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred"));
            }
        }

        [HttpPost("map")]
        public async Task<IActionResult> MapPost([FromBody] MapRequestDto dto)
        {
            if (dto == null) return BadRequest(ApiResponse<string>.Fail("Invalid request"));
            var taskId = await _importService.StartImportAsync(dto);
            return Accepted(ApiResponse<object>.Ok(new { TaskId = taskId }));
        }

        [HttpPost("errorcheck")]
        public async Task<IActionResult> ErrorCheck([FromBody] ErrorCheckRequestDto dto)
        {
            var progressId = Guid.NewGuid().ToString();
            await _importService.StartErrorCheckAsync(dto, progressId);
            return Ok(ApiResponse<object>.Ok(new { ProgressId = progressId }));
        }

        [HttpGet("progress-check/{id}")]
        public async Task<IActionResult> ProgressCheck(string id)
        {
            var percent = await _importService.GetProgressAsync(id);
            return Ok(ApiResponse<object>.Ok(new { Percent = percent }));
        }

        [HttpGet("errorcheck-result/{id}")]
        public async Task<IActionResult> ErrorCheckResult(string id)
        {
            var result = await _importService.GetErrorCheckResultAsync(id);
            if (result == null) return NotFound(ApiResponse<string>.Fail("No result found"));
            return Ok(ApiResponse<object>.Ok(new { result.ProgressId, result.SerializedErrors, result.CreatedAt }));
        }

        [HttpPost("confirm-map")]
        public async Task<IActionResult> ConfirmMap([FromBody] MapRequestDto dto)
        {
            if (dto == null) return BadRequest(ApiResponse<string>.Fail("Invalid request"));
            var result = await _importService.ConfirmMapAsync(dto);
            return Ok(ApiResponse<ConfirmMapResultDto>.Ok(result));
        }

        [HttpGet("upload")]
        public async Task<IActionResult> Upload([FromQuery] string taskId, [FromQuery] string URL = "")
        {
            var response = await _importService.CATRedirectURLAsync(taskId);

            return Ok(ApiResponse<CATUrlResponse>.Ok(response));
        }

        //[HttpGet("transfer")]
        //public IActionResult Transfer()
        //{
        //    return Ok(ApiResponse<string>.Ok("Transfer view is not supported in API"));
        //}

        //[HttpPost("transfer")]
        //public IActionResult TransferPost([FromQuery] int startId, [FromQuery] int endId)
        //{
        //    return Ok(ApiResponse<string>.Ok("Transfer invoked (no-op in API)"));
        //}

        [HttpGet("target-fields")]
        public IActionResult TargetFields([FromQuery] string type)
        {
            // Original logic used reflection on domain entities. For API, return NotImplemented or client-specific list.
            return NotFound(ApiResponse<string>.Fail("TargetFields endpoint not implemented. Implement domain mappings in API if needed."));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> Roles([FromQuery] string type, [FromQuery] int collectionId)
        {
            if (string.Equals(type, "Record", StringComparison.OrdinalIgnoreCase))
            {
                var roles = await _unitOfWork.RecordRoles.GetAllAsync();
                var names = roles.Select(r => r.Name);
                return Ok(ApiResponse<object>.Ok(names));
            }

            return NotFound(ApiResponse<string>.Fail("Roles endpoint type not supported."));
        }

        [HttpGet("mappings")]
        public async Task<IActionResult> Mappings([FromQuery] string name)
        {
            var all = await _unitOfWork.ImportSerializedMappings.FindAsync(m => m.MappingName == name);
            var mapping = all.FirstOrDefault();
            if (mapping == null)
                return Ok(ApiResponse<object>.Ok(new[] { new { } }));

            return Ok(ApiResponse<object>.Ok(new { Serialized = mapping.SerializedMappings }));
        }

        [HttpGet("progress")]
        public async Task<IActionResult> Progress([FromQuery] string taskId)
        {
            var percent = await _importService.GetProgressAsync(taskId);
            // tempList handling not implemented in stub
            return Ok(ApiResponse<object>.Ok(new { Percent = percent, TempList = string.Empty }));
        }

        [HttpGet("status/{taskId}")]
        public IActionResult GetStatusMessage(string taskId)
        {
            var msg = _importService.GetStatusMessage(taskId);
            return Ok(ApiResponse<object>.Ok(new { Message = msg }));
        }
    }
}
