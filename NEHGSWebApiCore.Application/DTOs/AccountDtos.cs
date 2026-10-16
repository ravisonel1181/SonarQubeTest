using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.DTOs
{
    public class LoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ManageDto
    {
        public bool HasLocalPassword { get; set; }
        public IEnumerable<UserLoginInfoDto> Logins { get; set; }
        public IEnumerable<ExternalProviderDto> ExternalLoginProviders { get; set; }
        public string StatusMessage { get; set; }
    }

    public class UserLoginInfoDto
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }

    public class ExternalProviderDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class AddPasswordDto
    {
        public string NewPassword { get; set; }
    }

    public class DisassociateDto
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }

    public class ExternalLoginInfoDto
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string DefaultUserName { get; set; }
    }

    public class ExternalLoginConfirmationDto
    {
        public string UserName { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }
}
