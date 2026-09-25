using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Services;
using Resend;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Notification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Implementation for adding infrastructure services
            services.Configure<ResendOptions>(configuration.GetSection("Resend"));
            services.AddHttpClient<ResendClient>();

            services.Configure<ResendClientOptions>(options =>
            {
                options.ApiToken =
                    configuration["Resend:apikey"]
                    ?? throw new InvalidOperationException(
                        "Resend API key is not configured.");
            });
            services.AddTransient<IResend, ResendClient>();

            services.AddTransient<IEmailSender, ResendEmailSender>();
            return services;
        }
    }
}
