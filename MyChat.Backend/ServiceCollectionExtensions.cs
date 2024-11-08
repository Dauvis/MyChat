using MyChat.Data;
using MyChat.Logic;
using Serilog;

namespace MyChat.Backend
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackendServices(this IServiceCollection services)
        {

            // Add AutoMapper with the profiles in your assembly
            services.AddAutoMapper(typeof(ServiceMappingProfile).Assembly);
            services.AddAutoMapper(typeof(DataMappingProfile).Assembly);            

            return services;
        }
    }
}
