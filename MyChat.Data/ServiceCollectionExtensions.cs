using Microsoft.Extensions.DependencyInjection;
using MyChat.Common.Interfaces;
using MyChat.Data.Abstraction;
using MyChat.Data.Interfaces;

namespace MyChat.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            // Add any services specific to the data layer here
            services.AddSingleton<IDataStoreClient, DataStoreClient> ();
            services.AddSingleton<IRepositoryFactory, RepositoryFactory> ();

            return services;
        }
    }
}
