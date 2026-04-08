using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Interfaces;
using StaySync.Infrastructure.BackgroundServices;
using StaySync.Infrastructure.Identity;
using StaySync.Infrastructure.Persistence;
using StaySync.Infrastructure.Providers.Ics;
using StaySync.Infrastructure.Services;

namespace StaySync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString + ";No Reset On Close=true;Max Auto Prepare=0"));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddHttpClient<IBookingProvider, IcsBookingProvider>();
        services.AddScoped<ICalendarSyncService, CalendarSyncService>();
        services.Configure<BackgroundSyncOptions>(configuration.GetSection(BackgroundSyncOptions.Section));
        services.AddHostedService<CalendarSyncWorker>();

        services.AddHttpContextAccessor();

        var jwtSettings = configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
