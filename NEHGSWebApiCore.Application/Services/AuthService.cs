using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using NEHGSWebApiCore.Application.Interfaces.Services;
using NEHGSWebApiCore.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NEHGSWebApiCore.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse> LoginAsync(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return ApiResponse.Fail("Invalid username", System.Net.HttpStatusCode.NotFound);

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            if (!result.Succeeded)
                return ApiResponse.Fail("Invalid username or password", System.Net.HttpStatusCode.BadRequest);

            var token = await CreateTokenForUserAsync(user.Id, user.UserName);
            var dto = new AuthResultDto
            {
                Token = token,
                User = new UserDto { Id = user.Id, UserName = user.UserName, Email = user.Email }
            };

            return ApiResponse.Success(dto);
        }

        public async Task<ApiResponse> RegisterAsync(string userName, string password)
        {
            var exists = await _userManager.FindByNameAsync(userName);
            if (exists != null)
                return ApiResponse.Fail("User already exists", System.Net.HttpStatusCode.BadRequest);

            var user = new ApplicationUser { UserName = userName };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return ApiResponse.Fail(string.Join("; ", result.Errors.Select(e => e.Description)), System.Net.HttpStatusCode.NotFound);

            var token = await CreateTokenForUserAsync(user.Id, user.UserName);
            var dto = new AuthResultDto
            {
                Token = token,
                User = new UserDto { Id = user.Id, UserName = user.UserName, Email = user.Email }
            };

            return ApiResponse.Success(dto);
        }

        public async Task<ApiResponse> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
                return ApiResponse.Fail("User not found", System.Net.HttpStatusCode.NotFound);

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded) 
                return ApiResponse.Fail(string.Join("; ", result.Errors.Select(e => e.Description)), System.Net.HttpStatusCode.BadRequest);

            return ApiResponse.Success(result, "Password changed");
        }

        public Task<string> CreateTokenForUserAsync(string userId, string userName)
        {
            var jwtSettings = _configuration.GetSection("JwtToken");
            var key = jwtSettings["SecretKey"] ?? "change_this_to_a_strong_key";
            var issuer = jwtSettings["Issuer"] ?? "NEHGS";
            var audience = jwtSettings["Audience"] ?? "NEHGS";
            var expiryMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var m) ? m : 60;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds);

            var written = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult(written);
        }
    }
}
