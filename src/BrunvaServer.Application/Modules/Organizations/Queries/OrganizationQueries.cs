using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;

namespace BrunvaServer.Application.Modules.Organizations.Queries;

public sealed record ListOrganizationsQuery(int Page, int PageSize) : IRequest<Result<Page<OrganizationResponse>>>;

public sealed class ListOrganizationsHandler(ICurrentUser currentUser, IOrganizationStore organizations)
    : IRequestHandler<ListOrganizationsQuery, Result<Page<OrganizationResponse>>>
{
    public async ValueTask<Result<Page<OrganizationResponse>>> Handle(ListOrganizationsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
            return Result<Page<OrganizationResponse>>.Failure(HttpStatus.Unauthorized, "unauthenticated", "Unauthenticated", "Sign in is required.");

        var page = await organizations.ListForUserAsync(userId, request.Page, request.PageSize, cancellationToken);
        return Result<Page<OrganizationResponse>>.Ok(page);
    }
}

public sealed record GetOrganizationQuery(Guid OrganizationId) : IRequest<Result<OrganizationResponse>>;

public sealed class GetOrganizationHandler(IOrganizationContext organizationContext, IOrganizationStore organizations)
    : IRequestHandler<GetOrganizationQuery, Result<OrganizationResponse>>
{
    public async ValueTask<Result<OrganizationResponse>> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        if (!organizationContext.IsSet || organizationContext.OrganizationId != request.OrganizationId)
            return Result<OrganizationResponse>.Failure(HttpStatus.Forbidden, "not_a_member", "Forbidden", "You do not have access to this organization.");

        var organization = await organizations.FindAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
            return Result<OrganizationResponse>.Failure(HttpStatus.NotFound, "organization_not_found", "Not found", "The organization was not found.");

        return Result<OrganizationResponse>.Ok(OrganizationInput.ToResponse(organization, organizationContext.Role));
    }
}

public sealed record ListMembersQuery(Guid OrganizationId, int Page, int PageSize) : IRequest<Result<Page<MemberResponse>>>;

public sealed class ListMembersHandler(IOrganizationContext organizationContext, IMembershipStore memberships)
    : IRequestHandler<ListMembersQuery, Result<Page<MemberResponse>>>
{
    public async ValueTask<Result<Page<MemberResponse>>> Handle(ListMembersQuery request, CancellationToken cancellationToken)
    {
        if (!organizationContext.IsSet || organizationContext.OrganizationId != request.OrganizationId)
            return Result<Page<MemberResponse>>.Failure(HttpStatus.Forbidden, "not_a_member", "Forbidden", "You do not have access to this organization.");

        var page = await memberships.ListAsync(request.OrganizationId, request.Page, request.PageSize, cancellationToken);
        return Result<Page<MemberResponse>>.Ok(page);
    }
}
