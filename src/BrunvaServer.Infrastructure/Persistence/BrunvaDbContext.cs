using System.Text;
using Microsoft.EntityFrameworkCore;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Domain.Common;
using BrunvaServer.Domain.Entities;

namespace BrunvaServer.Infrastructure.Persistence;

public sealed class BrunvaDbContext : DbContext
{
    private readonly IOrganizationContext _organizationContext;
    private readonly IClock _clock;

    public BrunvaDbContext(DbContextOptions<BrunvaDbContext> options, IOrganizationContext organizationContext, IClock clock)
        : base(options)
    {
        _organizationContext = organizationContext;
        _clock = clock;
    }
    public DbSet<User> Users => Set<User>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    private Guid? CurrentOrganizationId => _organizationContext.IsSet ? _organizationContext.OrganizationId : null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrunvaDbContext).Assembly);
        modelBuilder.Entity<OrganizationMember>().HasQueryFilter(member =>
            CurrentOrganizationId == null || member.OrganizationId == CurrentOrganizationId);
        ApplySnakeCase(modelBuilder);
        modelBuilder.Entity<User>().Property<uint>("Version").IsRowVersion();
        modelBuilder.Entity<Organization>().Property<uint>("Version").IsRowVersion();
        modelBuilder.Entity<OrganizationMember>().Property<uint>("Version").IsRowVersion();
        modelBuilder.Entity<RefreshToken>().Property<uint>("Version").IsRowVersion();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.MarkCreated(now);
            else if (entry.State == EntityState.Modified)
                entry.Entity.MarkUpdated(now);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ApplySnakeCase(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var table = entity.GetTableName();
            if (!string.IsNullOrWhiteSpace(table))
                entity.SetTableName(ToSnake(table));

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnake(property.Name));
        }
    }

    private static string ToSnake(string value)
    {
        var builder = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsUpper(character))
            {
                if (index > 0)
                    builder.Append('_');
                builder.Append(char.ToLowerInvariant(character));
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
