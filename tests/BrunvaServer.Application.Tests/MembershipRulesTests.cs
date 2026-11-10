using FluentAssertions;
using BrunvaServer.Domain.Enums;
using BrunvaServer.Domain.Membership;

namespace BrunvaServer.Application.Tests;

public sealed class MembershipRulesTests
{
    [Fact]
    public void Admin_cannot_assign_owner()
    {
        MembershipRules.CanAssign(MemberRole.Admin, MemberRole.Owner).Should().BeFalse();
        MembershipRules.CanAssign(MemberRole.Owner, MemberRole.Owner).Should().BeTrue();
        MembershipRules.CanAssign(MemberRole.Admin, MemberRole.Accountant).Should().BeTrue();
    }

    [Fact]
    public void Last_active_owner_cannot_be_removed()
    {
        MembershipRules.RemovesLastOwner(MemberRole.Owner, remainsActiveOwner: false, activeOwners: 1).Should().BeTrue();
        MembershipRules.RemovesLastOwner(MemberRole.Owner, remainsActiveOwner: true, activeOwners: 1).Should().BeFalse();
        MembershipRules.RemovesLastOwner(MemberRole.Owner, remainsActiveOwner: false, activeOwners: 2).Should().BeFalse();
        MembershipRules.RemovesLastOwner(MemberRole.Admin, remainsActiveOwner: false, activeOwners: 1).Should().BeFalse();
    }
}
