using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using MyChat.Common.DTO;
using MyChat.Common.Interfaces;
using MyChat.Data.Abstraction;
using MyChat.Data.Attributes;
using MyChat.Data.Models;
using System.Net;

namespace MyChat.Data.Repositories
{
    [DataStoreRepository("/partitionKey", "Main", "UserProfile")]
    public class UserProfileRepository : DataStoreRepository<UserProfileRepository>, IUserProfileRepository
    {
        public UserProfileRepository(Database _database, string containerId, string partitionKey, 
            ILogger<UserProfileRepository> logger, IServiceProvider serviceProvider, IMapper mapper) 
            : base(_database, containerId, partitionKey, logger, serviceProvider, mapper)
        {
        }

        public async Task<UserProfileDocumentDTO?> CreateAsync(UserInfoDTO userInfoDto, string authSource, string authUserId)
        {
            try
            {
                string userId = Guid.NewGuid().ToString();

                // Note: not including AuthId in UserInfo is intentional
                // The main reason is that api consumers should not be able to see it
                UserProfileDocument profile = new()
                {
                    Id = userId,
                    PartitionKey = authSource,
                    AuthUserId = authUserId,
                    UserInfo = new()
                    {
                        Id = userId,
                        Name = userInfoDto.Name,
                        Email = userInfoDto.Email
                    }
                };

                await Container.CreateItemAsync(profile, new PartitionKey(profile.PartitionKey));
                return Mapper.Map<UserProfileDocumentDTO>(profile);
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserProfileDocumentDTO?> GetAsync(string userId)
        {
            try
            {
                ItemResponse<UserProfileDocument> response = await Container.ReadItemAsync<UserProfileDocument>(userId, new PartitionKey("Entra"));

                return Mapper.Map<UserProfileDocumentDTO?>(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<UserProfileDocumentDTO?> GetByAuthUserIdAsync(string authUserId)
        {
            var queryText = "SELECT * FROM p WHERE p.AuthUserId = @authUserId";
            QueryDefinition queryDefinition = new QueryDefinition(queryText).WithParameter("@authUserId", authUserId);

            FeedIterator<UserProfileDocument> feedIterator = Container.GetItemQueryIterator<UserProfileDocument>(queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<UserProfileDocument> resultSet = await feedIterator.ReadNextAsync();
                UserProfileDocument? profile = resultSet.FirstOrDefault();

                return Mapper.Map<UserProfileDocumentDTO?>(profile);
            }

            return null;
        }

        public async Task<UserInfoDTO?> GetUserAsync(string userId)
        {
            var profile = await GetAsync(userId);
            var profileDto = Mapper.Map<UserProfileDocumentDTO?>(profile);

            return profileDto?.UserInfo ?? default;
        }

        public async Task<UserInfoDTO?> GetUserByAuthUserIdAsync(string authUserId)
        {
            var profile = await GetByAuthUserIdAsync(authUserId);
            var profileDto = Mapper.Map<UserProfileDocumentDTO?>(profile);

            return profileDto?.UserInfo ?? default;
        }
    }
}
