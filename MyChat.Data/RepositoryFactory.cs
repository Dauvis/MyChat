using MyChat.Common.Interfaces;
using MyChat.Data.Attributes;
using MyChat.Data.Interfaces;
using MyChat.Data.Repositories;
using System.Collections.Concurrent;
using System.Reflection;

namespace MyChat.Data
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private static readonly ConcurrentDictionary<Type, object> _repositoryCache = new();
        private static readonly ConcurrentDictionary<Type, Type> _repositoryImplementationMap = new();

        private readonly IDataStoreClient _dataStoreClient;
        private readonly IServiceProvider _serviceProvider;

        public RepositoryFactory(IDataStoreClient dataStoreClient, IServiceProvider serviceProvider)
        {
            _dataStoreClient = dataStoreClient;
            _serviceProvider = serviceProvider;
        }

        public async Task<T?> CreateAsync<T>() where T : class
        {
            if (_repositoryImplementationMap.IsEmpty)
            {
                BuildRepositoryImplementationMap();
            }

            var interfaceType = typeof(T);

            if (_repositoryCache.TryGetValue(interfaceType, out object? value))
            {
                return (T?)value;
            }

            if (!_repositoryImplementationMap.TryGetValue(interfaceType, out var repositoryType))
            {
                throw new InvalidOperationException($"No implementation mapping found for {interfaceType.Name}");
            }

            var repositoryAttribute = repositoryType.GetCustomAttribute<DataStoreRepositoryAttribute>() ??
                throw new InvalidOperationException($"{repositoryType.Name} lacks a {nameof(DataStoreRepositoryAttribute)}");

            var collection = await _dataStoreClient.GetCollectionAsync(repositoryAttribute.CollectionName);
            var getRepositoryMethod = collection.GetType().GetMethod("GetRepositoryAsync")!.MakeGenericMethod(repositoryType);
            var repositoryTask = (Task?)getRepositoryMethod.Invoke(collection, null);
            await repositoryTask!.ConfigureAwait(false);
            var resultProperty = repositoryTask.GetType().GetProperty("Result");

            var repository = (T?)resultProperty?.GetValue(repositoryTask);

            if (repository is not null)
            {
                _repositoryCache[interfaceType] = repository;
            }

            return repository;
        }

        private static void BuildRepositoryImplementationMap()
        {
            _repositoryImplementationMap[typeof(IUserProfileRepository)] = typeof(UserProfileRepository);
        }
    }
}
