using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyChat.Common.DTO;
using MyChat.Common.Enums;
using MyChat.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace MyChat.Security
{
    public class EntraTokenUtil: IEntraTokenUtil
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IRepositoryFactory _repositoryFactory;
        private IUserProfileRepository? _profileRepository;

        public EntraTokenUtil(IConfiguration configuration, IMapper mapper, IRepositoryFactory repositoryFactory)
        {
            _configuration = configuration;
            _mapper = mapper;
            _repositoryFactory = repositoryFactory;
        }

        public async Task<UserProfileDocumentDTO?> ProfileDocumentForIdTokenAsync(string? token)
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
                var authUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (authUserId is not null)
                {
                    var repository = await GetUserProfileRepositoryAsync();
                    var profile = await repository.GetByAuthUserIdAsync(authUserId);

                    if (profile is null)
                    {
                        var userInfoDto = new UserInfoDTO()
                        {
                            Name = principal.FindFirst("name")?.Value ?? "",
                            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? ""
                        };

                        profile = await repository.CreateAsync(userInfoDto, AuthenticationProvidersType.Entra.ToString(), authUserId);
                    }

                    return _mapper.Map<UserProfileDocumentDTO>(profile);
                }

                return null;
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

        protected async Task<IUserProfileRepository> GetUserProfileRepositoryAsync()
        {
            if (_profileRepository is null)
            {
                _profileRepository = await _repositoryFactory.CreateAsync<IUserProfileRepository>()
                    ?? throw new InvalidOperationException($"Failed to instantiate repository: {nameof(IUserProfileRepository)}");
            }

            return _profileRepository;
        }
    }
}
