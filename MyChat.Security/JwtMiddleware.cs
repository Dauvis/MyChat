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

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IIdentityTokenValidator tokenValidator)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            try
            {
                var userId = tokenValidator.ValidateToken(token);
                context.Items["UserId"] = userId;

                // TODO: Attach the user's base information to the context
                //if (userId != null)
                //{
                //    // Attach the user to the context.
                //    context.Items["User"] = userService.GetByUserId(userId.Value);
                //}
            }
            catch (SecurityTokenException)
            {
                // TODO: look into reporting more detailed information here
                _logger.LogError("Backend JWT validation failed");
                context.Items["UserId"] = "";
            }

            await _next(context);
        }
    }
}
