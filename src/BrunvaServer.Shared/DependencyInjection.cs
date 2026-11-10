using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BrunvaServer.Application;
using BrunvaServer.Infrastructure;

namespace BrunvaServer.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddBrunva(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }
}
