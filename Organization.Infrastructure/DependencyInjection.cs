using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Organization.Application.Interfaces;
using Organization.Infrastructure.Messaging;
using Organization.Infrastructure.Persistence;
using Organization.Infrastructure.Services;

namespace Organization.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// This is the extension method of the IServiceCollection to add the infrastructure layer services to the DI container. So that we can use that in the program.cs and not to make it more lengthy 
    /// </summary>
    /// <param name="services">This is the DI container where the services got registered</param>
    /// <param name="configuration">The appsettings instance</param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.Configure<RabbitMqSettings>(
         configuration.GetSection("RabbitMQ"));

        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IEventPublisher, EventService>();

        return services;
    }
}