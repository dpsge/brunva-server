using FluentValidation;
using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;

namespace BrunvaServer.Application.Modules.Identity.Commands;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result<EmptyBody>>, ITransactionalRequest;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(512);
    }
}

public sealed class LogoutHandler(
    ICurrentUser currentUser,
    IRefreshTokenStore refreshTokens,
    IRefreshTokenFactory tokenFactory,
    IClock clock)
    : IRequestHandler<LogoutCommand, Result<EmptyBody>>
{
    public async ValueTask<Result<EmptyBody>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
            return Result<EmptyBody>.Failure(HttpStatus.Unauthorized, "unauthenticated", "Unauthenticated", "Sign in is required.");

        var existing = await refreshTokens.FindByHashAsync(tokenFactory.Hash(request.RefreshToken), cancellationToken);
        if (existing is not null && existing.UserId == userId && existing.RevokedAt is null)
            existing.Revoke(clock.UtcNow);

        return Result<EmptyBody>.NoContent();
    }
}
