using System.Text.Json;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using BrunvaServer.Application.Behaviors;

namespace BrunvaServer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

        services.AddScoped<Modules.Identity.SessionIssuer>();
        services.AddValidatorsFromAssemblyContaining<ApplicationMarker>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return services;
    }
}

public sealed class ApplicationMarker;
