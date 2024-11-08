using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyChat.Common.DTO;
using MyChat.Common.Enums;
using MyChat.Common.Interfaces;

namespace MyChat.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IUserProfileService _userService;

        public ProfileController(IUserProfileService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var user = (UserInfoDTO?)HttpContext.Items["MyChatUser"];

            if (user == null)
            {
                var errorResponse = new ErrorResponseDTO(ErrorCodesType.NotAuthenticated, "User is not authenticated");
                return Unauthorized(errorResponse);
            }

            var response = await _userService.GetProfileAsync(user.Id);

            if (response == null)
            {
                var errorResponse = new ErrorResponseDTO(ErrorCodesType.InvalidProfile, "Profile for user not found.");
                return BadRequest(errorResponse);
            }

            // Make sure authUserId does not have a value
            response.AuthUserId = "";

            return Ok(response);
        }
    }
}
