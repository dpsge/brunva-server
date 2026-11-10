using BrunvaServer.Domain.Common;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Domain.Entities;

public sealed class OrganizationMember : Entity, IOrganizationEntity
{
    private OrganizationMember()
    {
    }

    public Guid OrganizationId { get; private set; }

    public Guid UserId { get; private set; }

    public MemberRole Role { get; private set; }

    public MemberStatus Status { get; private set; } = MemberStatus.Active;

    public static OrganizationMember Create(Guid organizationId, Guid userId, MemberRole role) =>
        new()
        {
            OrganizationId = organizationId,
            UserId = userId,
            Role = role
        };

    public void Change(MemberRole role, MemberStatus status)
    {
        Role = role;
        Status = status;
    }
}
