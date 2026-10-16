using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public class AuthResultDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }
}
