using Microsoft.AspNetCore.Mvc.Filters;
using BrunvaServer.Api.Problems;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Api.Filters;

public sealed class OrganizationContextFilter(
    IOrganizationContext organizationContext,
    ICurrentUser currentUser,
    IMembershipStore memberships,
    IUserStore users,
    IOrganizationStore organizations) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.TryGetValue("organizationId", out var value) || value is not Guid organizationId)
        {
            await next();
            return;
        }

        var cancellationToken = context.HttpContext.RequestAborted;
        if (currentUser.UserId is not Guid userId)
        {
            context.Result = ProblemResults.Create(context.HttpContext, StatusCodes.Status401Unauthorized, "unauthenticated", "Unauthenticated", "Sign in is required.");
            return;
        }

        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
        {
            context.Result = ProblemResults.Create(context.HttpContext, StatusCodes.Status401Unauthorized, "account_disabled", "Account disabled", "This account cannot sign in.");
            return;
        }

        var organization = await organizations.FindAsync(organizationId, cancellationToken);
        var member = await memberships.FindByUserAsync(organizationId, userId, cancellationToken);
        if (organization is null || organization.Status != OrganizationStatus.Active || member is null || member.Status != MemberStatus.Active)
        {
            context.Result = ProblemResults.Create(context.HttpContext, StatusCodes.Status403Forbidden, "not_a_member", "Forbidden", "You do not have access to this organization.");
            return;
        }

        organizationContext.Set(organizationId, member.Role);
        await next();
    }
}
