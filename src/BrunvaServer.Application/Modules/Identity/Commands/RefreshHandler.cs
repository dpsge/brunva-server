using FluentValidation;
using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Application.Modules.Identity.Commands;

public sealed record RefreshCommand(string RefreshToken)
    : IRequest<Result<AuthTokens>>, ITransactionalRequest;

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(512);
    }
}

public sealed class RefreshHandler(
    IRefreshTokenStore refreshTokens,
    IRefreshTokenFactory tokenFactory,
    IUserStore users,
    IClock clock,
    IUnitOfWork unitOfWork,
    SessionIssuer sessions)
    : IRequestHandler<RefreshCommand, Result<AuthTokens>>
{
    public async ValueTask<Result<AuthTokens>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var existing = await refreshTokens.FindByHashAsync(tokenFactory.Hash(request.RefreshToken), cancellationToken);
        if (existing is null)
            return Unauthorized("invalid_refresh_token", "The refresh token is not valid.");

        if (existing.RevokedAt is not null)
        {
            await refreshTokens.RevokeActiveAsync(existing.UserId, clock.UtcNow, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Unauthorized("refresh_reused", "The refresh token was already used. Sign in again.");
        }

        if (existing.ExpiresAt <= clock.UtcNow)
            return Unauthorized("refresh_expired", "The refresh token has expired.");

        var user = await users.FindByIdAsync(existing.UserId, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
            return Unauthorized("account_disabled", "This account cannot sign in.");

        return Result<AuthTokens>.Ok(sessions.Rotate(user, existing));
    }

    private static Result<AuthTokens> Unauthorized(string code, string detail) =>
        Result<AuthTokens>.Failure(HttpStatus.Unauthorized, code, "Unauthorized", detail);
}
