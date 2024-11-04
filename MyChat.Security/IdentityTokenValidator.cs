using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.IdentityModel.Tokens;
using MyChat.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace MyChat.Security
{
    public class IdentityTokenValidator : IIdentityTokenValidator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IIdentityTokenValidator> _logger;
        private readonly string _secretKey;
        private readonly string _validIssuer;
        private readonly string _validAudience;

        public IdentityTokenValidator(IConfiguration configuration, ILogger<IIdentityTokenValidator> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _secretKey = configuration["JWT_SECRET_KEY"] ?? throw new ArgumentNullException("JWT secret key has not been set");
            _validIssuer = configuration["Jwt:ValidIssuer"] ?? throw new ArgumentNullException("JWT valid issuer has not been set");
            _validAudience = configuration["Jwt:ValidAudience"] ?? throw new ArgumentNullException("JWT valid audience has not been set");
        }

        public async Task<string?> ValidateTokenEntraAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = await GetSigningKeysAsync(),
                ValidateIssuer = true,
                ValidIssuer = $"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}/v2.0",
                ValidateAudience = true,
                ValidAudience = _configuration["AzureAd:ClientId"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero  // Set to zero if you want the token expiration to be exact
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                // Retrieve user ID from claims if needed
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return userIdClaim;
            }
            catch
            {
                return null;
            }
        }

        private async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
        {
            using var httpClient = new HttpClient();

            // Fetch the OpenID configuration
            var discoveryDocumentUrl = $"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}/v2.0/.well-known/openid-configuration";
            var discoveryDocumentString = await httpClient.GetStringAsync(discoveryDocumentUrl);

            // Deserialize the OpenID configuration
            var discoveryDocument = JsonSerializer.Deserialize<JsonElement>(discoveryDocumentString);

            // Extract the JWKS URI from the OpenID configuration
            if (discoveryDocument.TryGetProperty("jwks_uri", out var jwksUriElement))
            {
                var jwksUri = jwksUriElement.GetString();

                // Retrieve the JWKS from the JWKS URI
                var jwksResponse = await httpClient.GetStringAsync(jwksUri);
                var jwks = JsonSerializer.Deserialize<JsonWebKeySet>(jwksResponse);

                return jwks?.Keys ?? [];
            }

            throw new InvalidOperationException("Failed to retrieve JWKS URI from discovery document.");
        }
        public ClaimsPrincipal? ValidateToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(_secretKey); 

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
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
                return principal;
            }
            catch (Exception ex)
            {
                // Handle validation failure
                _logger.LogError("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }
    }
}
