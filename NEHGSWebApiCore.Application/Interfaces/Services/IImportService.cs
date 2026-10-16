using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using NEHGSWebApiCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Interfaces.Services
{
    public interface IImportService
    {
        Task<IEnumerable<string>> GetCsvPathsAsync(string parentDirectory);
        Task<PreviewResponseDto> GetPreviewAsync(string csvPath, string metadataPath);
        Task<string> StartImportAsync(MapRequestDto request);
        Task StartErrorCheckAsync(ErrorCheckRequestDto request, string progressId);
        Task<int> GetProgressAsync(string taskId);
        string GetStatusMessage(string taskId);
        Task<ConfirmMapResultDto> ConfirmMapAsync(MapRequestDto request);
        Task<ErrorCheckResult> GetErrorCheckResultAsync(string progressId);
        Task<CATUrlResponse> CATRedirectURLAsync(string taskId);
    }
}
