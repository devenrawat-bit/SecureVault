using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
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

            // Configure RabbitMQ settings.
            services.Configure<RabbitMqSettings>(
                configuration.GetSection("RabbitMq"));

            // Configure the Notification database.
             services.AddDbContext<NotificationDbContext>(options => options.UseSqlServer( configuration.GetConnectionString("NotificationDb")));

            services.AddTransient<IEmailSender, SmtpEmailSender>();
            services.AddTransient<IInvitationEmailService, InvitationEmailService>();
            services.AddScoped<IEmailDeliveryTracker, EmailDeliveryTracker>();
            services.AddTransient<IPasswordResetEmailService, PasswordResetEmailService>();

            // Start the RabbitMQ consumer when the application starts.
            services.AddHostedService<RabbitMqConsumer>();
            return services;
        }
    }
}
