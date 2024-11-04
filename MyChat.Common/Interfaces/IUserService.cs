using MyChat.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.Interfaces
{
    public interface IUserService
    {
        Task<AuthenticationResponseDTO> AuthenticateAsync(AuthenticationRequestDTO request);
    }
}
