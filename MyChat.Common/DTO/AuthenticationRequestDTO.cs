using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.DTO
{
    public class AuthenticationRequestDTO
    {
        public AuthenticationRequestDTO(string authProvider, string identityToken)
        {
            AuthProvider = authProvider;
            IdentityToken = identityToken;
        }

        public string AuthProvider { get; set; }
        public string IdentityToken { get; set; }
    }
}
