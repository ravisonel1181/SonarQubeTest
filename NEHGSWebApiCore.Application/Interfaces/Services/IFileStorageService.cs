using Microsoft.AspNetCore.Http;
using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<ApiResponse> SaveCsvAsync(IFormFile file);
    }
}
