using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyChat.Data.Attributes;
using MyChat.Data.Interfaces;
using System.Reflection;

namespace MyChat.Data.Abstraction
{
    public class DataStoreCollection : IDataStoreCollection
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _configuration;
        private readonly string _databaseName;
        private readonly ILogger<DataStoreCollection> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly string _databaseId;
        private Database? _database;

        public DataStoreCollection(CosmosClient cosmosClient, IConfiguration configuration, string databaseName, 
            ILogger<DataStoreCollection> logger, IServiceProvider serviceProvider, IMapper mapper)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _databaseName = databaseName;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _databaseId = _configuration[$"Cosmos:Databases:{databaseName}"] ?? "";

            if (string.IsNullOrEmpty(_databaseName))
            {
                throw new ArgumentNullException(nameof(_databaseName));
            }
        }

        public async Task InitializeAsync()
        {
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
        }

        public async Task<DataStoreRepository<T>> GetRepositoryAsync<T>() where T : DataStoreRepository<T>
        {
            var repositoryType = typeof(T);

            if (!repositoryType.IsSubclassOf(typeof(DataStoreRepository<T>)))
            {
                throw new InvalidOperationException($"{repositoryType.Name} is not a supported repository class");
            }

            if (_database is null)
            {
                throw new InvalidOperationException($"{nameof(DataStoreCollection)} for {_databaseName} has not bee initialized");
            }

            var attribute = repositoryType.GetCustomAttribute<DataStoreRepositoryAttribute>() ?? 
                throw new InvalidOperationException($"{repositoryType.Name} lacks a {nameof(DataStoreRepositoryAttribute)}");

            string repositoryName = string.IsNullOrEmpty(attribute.RepositoryName) ? repositoryType.Name : attribute.RepositoryName;
            var repositoryLogger = _serviceProvider.GetRequiredService<ILogger<T>>();
            object[] ctorArgs = [_database, repositoryName, attribute.PartitionKey, repositoryLogger, _serviceProvider, _mapper];

            var repository = (DataStoreRepository<T>?)Activator.CreateInstance(repositoryType, ctorArgs) ??
                throw new InvalidOperationException($"Failed to instantiate {repositoryType.Name}");

            await repository.InitializeAsync();

            return repository;
        }
    }
}
