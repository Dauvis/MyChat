using MyChat.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.DTO
{
    public class ErrorResponseDTO
    {
        public ErrorResponseDTO(ErrorCodesType errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public ErrorCodesType ErrorCode { get; }
        public string ErrorMessage { get; }
    }
}
