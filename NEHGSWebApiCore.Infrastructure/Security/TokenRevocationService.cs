using NEHGSWebApiCore.Application.Interfaces.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Infrastructure.Security
{
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly ConcurrentDictionary<string, DateTime> _revoked = new ConcurrentDictionary<string, DateTime>();

        public void RevokeToken(string jti)
        {
            if (string.IsNullOrEmpty(jti)) return;
            _revoked[jti] = DateTime.UtcNow;
        }

        public bool IsRevoked(string jti)
        {
            if (string.IsNullOrEmpty(jti)) return false;
            return _revoked.ContainsKey(jti);
        }
    }
}
