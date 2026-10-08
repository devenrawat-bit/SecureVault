using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Services;
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
            services.Configure<SmtpOptions>(
            configuration.GetSection("Smtp"));

            services.AddTransient<IEmailSender, SmtpEmailSender>();
            return services;
        }
    }
}
