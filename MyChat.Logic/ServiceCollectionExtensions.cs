using Microsoft.Extensions.DependencyInjection;
using MyChat.Common.Interfaces;
using MyChat.Logic.Services;

namespace MyChat.Backend
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogicServices(this IServiceCollection services)
        {
            // Add any services specific to the logic layer here
            services.AddSingleton<IUserProfileService, UserProfileService>();

            return services;
        }
    }
}
