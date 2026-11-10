using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;

namespace BrunvaServer.Application.Modules.Identity.Queries;

public sealed record GetMeQuery : IRequest<Result<MeResponse>>;

public sealed record MeResponse(UserProfile User, IReadOnlyList<OrganizationResponse> Organizations);

public sealed class GetMeHandler(ICurrentUser currentUser, IUserStore users, IOrganizationStore organizations)
    : IRequestHandler<GetMeQuery, Result<MeResponse>>
{
    public async ValueTask<Result<MeResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
            return Result<MeResponse>.Failure(HttpStatus.Unauthorized, "unauthenticated", "Unauthenticated", "Sign in is required.");

        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<MeResponse>.Failure(HttpStatus.Unauthorized, "unauthenticated", "Unauthenticated", "Sign in is required.");

        var memberships = await organizations.ListForUserAsync(userId, 1, PageWindow.MaxSize, cancellationToken);
        return Result<MeResponse>.Ok(new MeResponse(UserMapper.ToProfile(user), memberships.Items));
    }
}
