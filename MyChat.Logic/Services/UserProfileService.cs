using AutoMapper;
using MyChat.Common.DTO;
using MyChat.Common.Enums;
using MyChat.Common.Interfaces;
using MyChat.Data.Interfaces;
using System.Security.Claims;

namespace MyChat.Logic.Services
{
    public class UserProfileService: IUserProfileService
    {
        private readonly IMyChatTokenUtil _myChatTokenUtil;
        private readonly IEntraTokenUtil _entraTokenUtil;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IMapper _mapper;
        private IUserProfileRepository? _userProfileRepository;

        public UserProfileService(IMyChatTokenUtil myChatTokenUtil, IEntraTokenUtil entraTokenUtil, IRepositoryFactory repositoryFactory, IMapper mapper) 
        {
            _myChatTokenUtil = myChatTokenUtil;
            _entraTokenUtil = entraTokenUtil;
            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
        }

        protected async Task<IUserProfileRepository> GetUserProfileRepositoryAsync()
        {
            if (_userProfileRepository is null)
            {
                _userProfileRepository = await _repositoryFactory.CreateAsync<IUserProfileRepository>()
                    ?? throw new InvalidOperationException($"Failed to instantiate repository: {nameof(IUserProfileRepository)}");
            }

            return _userProfileRepository;
        }

        public async Task<AuthenticationResponseDTO> AuthenticateAsync(AuthenticationRequestDTO request)
        {
            var profileDto = await _entraTokenUtil.ProfileDocumentForIdTokenAsync(request.IdentityToken);

            if (profileDto is null)
            {
                return new AuthenticationResponseDTO(request.AuthProvider, "")
                {
                    ErrorCode = ErrorCodesType.AuthenticationFailed,
                    ErrorMessage = "Identity was not able to be verified."
                };
            }
            else
            {
                var jwt = _myChatTokenUtil.GenerateToken(CreateClaimsPrincipal(profileDto));

                return new AuthenticationResponseDTO(request.AuthProvider, jwt);
            }            
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(UserProfileDocumentDTO profileDto)
        {
            List<Claim> claims = [];
            claims.Add(new(ClaimTypes.NameIdentifier, profileDto.UserInfo.Id));
            claims.Add(new(ClaimTypes.Name, profileDto.UserInfo.Name));
            claims.Add(new(ClaimTypes.Email, profileDto.UserInfo.Email));

            // TODO: Add user roles as claims, if any

            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }

        public async Task<UserInfoDTO?> GetAsync(string userId)
        {
            var userRepository = await GetUserProfileRepositoryAsync();
            var userInfo = await userRepository.GetUserAsync(userId);

            return _mapper.Map<UserInfoDTO?>(userInfo);
        }

        public async Task<UserInfoDTO?> GetByAuthUserIdAsync(string authUserId)
        {
            var userRepository = await GetUserProfileRepositoryAsync();
            var userInfo = await userRepository.GetUserByAuthUserIdAsync(authUserId);

            return _mapper.Map<UserInfoDTO?>(userInfo);
        }

        public async Task<UserProfileDocumentDTO?> GetProfileAsync(string userId)
        {
            var userRepository = await GetUserProfileRepositoryAsync();
            var profile = await userRepository.GetAsync(userId);

            return _mapper.Map<UserProfileDocumentDTO?>(profile);
        }

        public async Task<UserProfileDocumentDTO?> GetProfileByAuthUserIdAsync(string authUserId)
        {
            var userRepository = await GetUserProfileRepositoryAsync();
            var profile = await userRepository.GetByAuthUserIdAsync(authUserId);

            return _mapper.Map<UserProfileDocumentDTO?>(profile);
        }
    }
}
