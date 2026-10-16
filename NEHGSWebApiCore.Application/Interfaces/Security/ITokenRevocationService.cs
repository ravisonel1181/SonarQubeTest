using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Interfaces.Security
{
    public interface ITokenRevocationService
    {
        void RevokeToken(string jti);
        bool IsRevoked(string jti);
    }
}
