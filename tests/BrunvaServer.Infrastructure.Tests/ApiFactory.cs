using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using BrunvaServer.Infrastructure.Persistence;

namespace BrunvaServer.Infrastructure.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private string _connectionString = string.Empty;

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__Brunva", _connectionString);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", SigningKey);
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BrunvaDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Brunva"] = _connectionString,
                ["Jwt:SigningKey"] = SigningKey,
                ["Jwt:Issuer"] = "brunva",
                ["Jwt:Audience"] = "brunva"
            });
        });
    }

    private const string SigningKey = "integration-test-signing-key-32b";
}

[CollectionDefinition(ApiCollection.Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
