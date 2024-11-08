using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyChat.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyChat.Security
{
    public class MyChatTokenUtil: IMyChatTokenUtil
    {
        private readonly string _secretKey;
        private readonly string _validIssuer;
        private readonly string _validAudience;
        private readonly ILogger<MyChatTokenUtil> _logger;

        public MyChatTokenUtil(IConfiguration configuration, ILogger<MyChatTokenUtil> logger)
        {
            _logger = logger;
            _secretKey = configuration["JWT_SECRET_KEY"] ?? throw new ArgumentNullException("JWT secret key has not been set");
            _validIssuer = configuration["Jwt:ValidIssuer"] ?? throw new ArgumentNullException("JWT valid issuer has not been set");
            _validAudience = configuration["Jwt:ValidAudience"] ?? throw new ArgumentNullException("JWT valid audience has not been set");
        }

        public string? ValidateToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(GetKeyBytes()),
                ValidateIssuer = true,
                ValidIssuer = _validIssuer,
                ValidateAudience = true,
                ValidAudience = _validAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                return userId;
            }
            catch (Exception ex)
            {
                // Handle validation failure
                _logger.LogError("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }

        public string GenerateToken(ClaimsPrincipal claimsPrincipal)
        {
            var securityKey = new SymmetricSecurityKey(GetKeyBytes());
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claimsPrincipal.Claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        private byte[] GetKeyBytes()
        {
            return Encoding.UTF8.GetBytes(_secretKey);
        }
    }
}
