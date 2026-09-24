using Auth.Application.Interfaces;
using Auth.Infrastructure.Authentication;
using Auth.Infrastructure.Identity;
using Auth.Infrastructure.Messaging;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;

namespace Auth.Infrastructure;

/// <summary>
/// This is a static class whose job is to contain our Infrastructure registration methods.
/// We're doing this so Auth.API doesn't need to know all the implementation details.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, //extension method of the iservicecollection interface
        IConfiguration configuration) //take from the appsettings.json file
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddDataProtection();

        services
     .AddIdentityCore<ApplicationUser>()
     .AddRoles<IdentityRole<Guid>>()
     .AddEntityFrameworkStores<AppDbContext>() //This connects Identity to EF Core.
     .AddDefaultTokenProviders(); //This enables Identity's built-in token providers. It provide the operations such as password reset, email confirmation, Two factor authentication, etc.

        services.Configure<JwtSettings>(
    configuration.GetSection("Jwt"));

        var jwtSettings = configuration
            .GetSection("Jwt")
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

        //for the rabbit mq settings
        services.Configure<RabbitMqSettings>(
        configuration.GetSection("RabbitMQ")); //this will map the RabbitMqSettings class to the RabbitMQ section in the appsettings.json file
        services.AddHostedService<RabbitMqConsumer>();

        services.AddAuthorization();

        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<JwtTokenService>();
        return services;
    }
}