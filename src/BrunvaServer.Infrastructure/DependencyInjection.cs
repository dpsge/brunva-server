using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Infrastructure.Authentication;
using BrunvaServer.Infrastructure.Persistence;

namespace BrunvaServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Brunva");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'Brunva' is required.");

        services.AddOptions<AuthSettings>()
            .Bind(configuration.GetSection(AuthSettings.Section))
            .Validate(settings => Encoding.UTF8.GetByteCount(settings.SigningKey) >= 32, "Jwt:SigningKey must be at least 32 bytes.")
            .ValidateOnStart();

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        services.AddDbContext<BrunvaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<IOrganizationStore, OrganizationStore>();
        services.AddScoped<IMembershipStore, MembershipStore>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        return services;
    }
}
