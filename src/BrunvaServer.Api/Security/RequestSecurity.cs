using System.Security.Claims;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Api.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}

public sealed class OrganizationContext : IOrganizationContext
{
    public bool IsSet { get; private set; }

    public Guid OrganizationId { get; private set; }

    public MemberRole Role { get; private set; }

    public void Set(Guid organizationId, MemberRole role)
    {
        OrganizationId = organizationId;
        Role = role;
        IsSet = true;
    }
}
