using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using NEHGSWebApiCore.Application.Common;
using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using NEHGSWebApiCore.Application.Interfaces;
using NEHGSWebApiCore.Application.Interfaces.Security;
using NEHGSWebApiCore.Application.Interfaces.Services;
using NEHGSWebApiCore.Domain.Entities;
using NEHGSWebApiCore.Infrastructure.Security;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenRevocationService _tokenRevocationService;
        private readonly ILogger<AccountController> _logger;

        private const string XsrfKey = "XsrfId";

        public AccountController(IAuthService authService, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ITokenRevocationService tokenRevocationService,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenRevocationService = tokenRevocationService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse.Fail("Invalid request", System.Net.HttpStatusCode.BadRequest));

            var result = await _authService.LoginAsync(model.UserName, model.Password);
            if (result.StatusCode != HttpStatusCode.OK) 
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ApiResponse.Fail("Invalid request", System.Net.HttpStatusCode.BadRequest));

            var result = await _authService.RegisterAsync(model.UserName, model.Password);
            if (result.StatusCode != HttpStatusCode.OK) 
                return BadRequest(result);

            return Created("", result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) 
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            if (result.StatusCode != HttpStatusCode.OK) 
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("manage")]
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) 
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
                return Unauthorized();

            var logins = await _userManager.GetLoginsAsync(user);
            var providers = await _signInManager.GetExternalAuthenticationSchemesAsync();

            var result = new ManageDto
            {
                HasLocalPassword = !string.IsNullOrEmpty(user.PasswordHash),
                Logins = logins.Select(l => new UserLoginInfoDto { LoginProvider = l.LoginProvider, ProviderKey = l.ProviderKey }).ToArray(),
                ExternalLoginProviders = providers.Select(p => new ExternalProviderDto { Name = p.Name, DisplayName = p.DisplayName }).ToArray(),
                StatusMessage = string.Empty
            };

            return Ok(ApiResponse.Success(result));
        }

        [HttpPost("add-password")]
        [Authorize]
        public async Task<IActionResult> AddPassword([FromBody] AddPasswordDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) 
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
                return Unauthorized();

            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!result.Succeeded) 
                return BadRequest();

            return Ok(ApiResponse.Success(result));
        }

        [HttpPost("disassociate")]
        [Authorize]
        public async Task<IActionResult> Disassociate([FromBody] DisassociateDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded) return BadRequest();

            return Ok(ApiResponse.Success(result));
        }

        //[HttpPost("logout")]
        //[Authorize]
        //public async Task<IActionResult> Logout()
        //{
        //    // For JWT, revoke current token by jti claim
        //    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        //    if (!string.IsNullOrEmpty(jti))
        //    {
        //        _tokenRevocationService.RevokeToken(jti);
        //    }

        //    // Also sign out cookie if present
        //    await HttpContext.SignOutAsync();

        //    return Ok(ApiResponse.Success(jti, "Signed out"));
        //}

        [HttpGet("remove-account-list")]
        [Authorize]
        public async Task<IActionResult> RemoveAccountList()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var logins = await _userManager.GetLoginsAsync(user);
            var showRemove = !string.IsNullOrEmpty(user.PasswordHash) || logins.Count > 1;

            var data = new
            {
                Logins = logins.Select(l => new UserLoginInfoDto { LoginProvider = l.LoginProvider, ProviderKey = l.ProviderKey }),
                ShowRemoveButton = showRemove
            };

            return Ok(ApiResponse.Success(data));
        }

        [HttpGet("external-login")]
        [AllowAnonymous]
        public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl = null)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback([FromQuery] string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BadRequest();
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                // create token
                var token = await _authService.CreateTokenForUserAsync(user.Id, user.UserName);
                return Ok(ApiResponse.Success(new AuthResultDto { Token = token, User = new UserDto { Id = user.Id, UserName = user.UserName, Email = user.Email } }));
            }
            else
            {
                // If the user does not have an account, then prompt the client to create an account
                var defaultUserName = info.Principal.FindFirst(ClaimTypes.Name)?.Value ?? info.ProviderKey;
                var dto = new ExternalLoginInfoDto { LoginProvider = info.LoginProvider, ProviderKey = info.ProviderKey, DefaultUserName = defaultUserName };
                return Ok(ApiResponse.Success(dto));
            }
        }

        [HttpPost("external-login-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmation([FromBody] ExternalLoginConfirmationDto model, [FromQuery] string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated) return BadRequest();

            var user = new ApplicationUser { UserName = model.UserName };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded) return BadRequest();

            var loginInfo = new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.LoginProvider);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded) return BadRequest();

            // create token and return
            var token = await _authService.CreateTokenForUserAsync(user.Id, user.UserName);
            return Ok(ApiResponse.Success(new AuthResultDto { Token = token, User = new UserDto { Id = user.Id, UserName = user.UserName, Email = user.Email } }));
        }

        [HttpPost("link-login")]
        [Authorize]
        public IActionResult LinkLogin([FromQuery] string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var redirectUrl = Url.Action("LinkLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
            return Challenge(properties, provider);
        }

        [HttpGet("link-login-callback")]
        [Authorize]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var info = await _signInManager.GetExternalLoginInfoAsync(userId);
            if (info == null) return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded) return BadRequest();

            return Ok(ApiResponse.Success(result));
        }
    }
}
