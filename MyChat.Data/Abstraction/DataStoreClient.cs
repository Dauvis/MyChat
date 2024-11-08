using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyChat.Data.Interfaces;

namespace MyChat.Data.Abstraction
{
    public class DataStoreClient : IDataStoreClient
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataStoreClient> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly string _connectionString;
        private readonly CosmosClient _cosmosClient;

        public DataStoreClient(IConfiguration configuration, ILogger<DataStoreClient> logger, IServiceProvider serviceProvider, IMapper mapper)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _connectionString = _configuration["COSMOS_CONNECT_INFO"] ?? "";

            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogCritical("Connection string has not be set up");
                throw new ArgumentNullException(nameof(_connectionString));
            }

            _cosmosClient = new CosmosClient(_connectionString);
        }

        public async Task<IDataStoreCollection> GetCollectionAsync(string databaseName)
        {
            var collectionLogger = _serviceProvider.GetRequiredService<ILogger<DataStoreCollection>>();            
            var collection = new DataStoreCollection(_cosmosClient, _configuration, databaseName, collectionLogger, _serviceProvider, _mapper);
            await collection.InitializeAsync();

            return collection;
        }
    }
}
