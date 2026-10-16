
using Microsoft.AspNetCore.Identity;

namespace NEHGSWebApiCore.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
    }
}
