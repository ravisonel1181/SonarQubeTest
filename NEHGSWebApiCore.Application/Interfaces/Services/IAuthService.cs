using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<ApiResponse> LoginAsync(string userName, string password);
        Task<ApiResponse> RegisterAsync(string userName, string password);
        Task<ApiResponse> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<string> CreateTokenForUserAsync(string userId, string userName);
    }
}
