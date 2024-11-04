using Microsoft.Extensions.DependencyInjection;

namespace MyChat.Backend
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            // Add any services specific to the common layer here

            return services;
        }
    }
}
