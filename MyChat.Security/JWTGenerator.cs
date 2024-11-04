using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyChat.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyChat.Security
{
    public class JWTGenerator : IJwtGenerator
    {
        private readonly string _secretKey;
        private readonly string _validIssuer;
        private readonly string _validAudience;

        public JWTGenerator(IConfiguration configuration)
        {
            _secretKey = configuration["JWT_SECRET_KEY"] ?? throw new ArgumentNullException("JWT secret key has not been set");
            _validIssuer = configuration["Jwt:ValidIssuer"] ?? throw new ArgumentNullException("JWT valid issuer has not been set");
            _validAudience = configuration["Jwt:ValidAudience"] ?? throw new ArgumentNullException("JWT valid audience has not been set");
        }

        public string GenerateJwtToken(ClaimsPrincipal claimsPrincipal)
        {            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claimsPrincipal.Claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}
