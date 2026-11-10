using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Domain.Enums;
using BrunvaServer.Infrastructure.Authentication;

namespace BrunvaServer.Infrastructure.Persistence;

public sealed class BrunvaDbContextFactory : IDesignTimeDbContextFactory<BrunvaDbContext>
{
    public BrunvaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BrunvaDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=brunva;Username=brunva;Password=brunva")
            .Options;
        return new BrunvaDbContext(options, new DesignOrganizationContext(), new SystemClock());
    }

    private sealed class DesignOrganizationContext : IOrganizationContext
    {
        public bool IsSet => false;

        public Guid OrganizationId => Guid.Empty;

        public MemberRole Role => MemberRole.Viewer;

        public void Set(Guid organizationId, MemberRole role)
        {
        }
    }
}
