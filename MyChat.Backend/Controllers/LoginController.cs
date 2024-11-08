using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyChat.Common.DTO;
using MyChat.Common.Enums;
using MyChat.Common.Interfaces;

namespace MyChat.Backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly IUserProfileService _userService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IUserProfileService userService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync([FromBody] AuthenticationRequestDTO request)
        {
            try
            {
                var response = await _userService.AuthenticateAsync(request);

                if (response.ErrorCode == ErrorCodesType.None)
                {
                    return Ok(response);
                }
                else
                {
                    ErrorResponseDTO errorResponse = new(response.ErrorCode, response.ErrorMessage);

                    if (errorResponse.ErrorCode == ErrorCodesType.AuthenticationFailed)
                    {
                        return Unauthorized(errorResponse);
                    }

                    return new ObjectResult(errorResponse)
                    {
                        StatusCode = 403
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Exception while attempting to authenticate user from {AuthProvider}: {Message}", request.AuthProvider, e.Message);
                ErrorResponseDTO errorResponse = new(ErrorCodesType.UnknownError, "Unknown error detected");
                return BadRequest(errorResponse);
            }
        }
    }
}
