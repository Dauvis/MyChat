using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyChat.Common.Interfaces;

namespace MyChat.Security
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;
        private readonly IUserProfileService _userService;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger, IUserProfileService userService)
        {
            _next = next;
            _logger = logger;
            _userService = userService;
        }

        public async Task Invoke(HttpContext context, IMyChatTokenUtil tokenUtil)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            try
            {
                var userId = tokenUtil.ValidateToken(token);
                context.Items["MyChatUserId"] = userId;

                if (userId != null)
                {
                    // Attach the user to the context.
                    context.Items["MyChatUser"] = await _userService.GetAsync(userId);
                }
            }
            catch (SecurityTokenException)
            {
                // TODO: look into reporting more detailed information here
                _logger.LogError("Backend JWT validation failed");
                context.Items["MyChatUserId"] = "";
            }

            await _next(context);
        }
    }
}
