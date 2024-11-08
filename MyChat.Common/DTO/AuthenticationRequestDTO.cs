using MyChat.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.DTO
{
    public class AuthenticationRequestDTO
    {
        public AuthenticationRequestDTO(AuthenticationProvidersType authProvider, string identityToken)
        {
            AuthProvider = authProvider;
            IdentityToken = identityToken;
        }

        public AuthenticationProvidersType AuthProvider { get; set; }
        public string IdentityToken { get; set; }
    }
}
