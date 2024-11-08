using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace MyChat.Data.Abstraction
{
    public class DataStoreRepository<T>
    {
        private readonly Database _database;

        protected readonly string _containerId;
        private readonly string _partitionKey;
        private readonly ILogger<T> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        protected Container? _container;

        public DataStoreRepository(Database _database, string containerId, string partitionKey, ILogger<T> logger, 
            IServiceProvider serviceProvider, IMapper mapper)
        {
            this._database = _database;
            _containerId = containerId;
            _partitionKey = partitionKey;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
        }

        public async Task InitializeAsync()
        {
            _container = await _database.CreateContainerIfNotExistsAsync(_containerId, _partitionKey);
        }

        protected Container Container
        {
            get
            {
                return _container ?? throw new InvalidOperationException($"{GetType().Name} was not intialized");
            }
        }

        protected ILogger<T> Logger => _logger;
        protected IServiceProvider ServiceProvider => _serviceProvider;
        protected IMapper Mapper => _mapper;
    }
}