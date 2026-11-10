using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Domain.Membership;

public static class MembershipRules
{
    public static bool CanManageOrganization(MemberRole role) =>
        role is MemberRole.Owner or MemberRole.Admin;

    public static bool CanAssign(MemberRole actor, MemberRole target) =>
        actor == MemberRole.Owner || (actor == MemberRole.Admin && target != MemberRole.Owner);

    public static bool CanChange(MemberRole actor, MemberRole current, MemberRole next) =>
        CanAssign(actor, current) && CanAssign(actor, next);

    public static bool RemovesLastOwner(MemberRole role, bool remainsActiveOwner, int activeOwners) =>
        role == MemberRole.Owner && activeOwners <= 1 && !remainsActiveOwner;
}
