﻿using Microsoft.Extensions.DependencyInjection;
using MyChat.Common.Interfaces;
using MyChat.Security;

namespace MyChat.Backend
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services)
        {
            // Add any services specific to the security layer here
            services.AddSingleton<IMyChatTokenUtil, MyChatTokenUtil>();
            services.AddSingleton<IEntraTokenUtil, EntraTokenUtil>();

            return services;
        }
    }
}