using FluentValidation;
using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Application.Modules.Identity.Commands;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<Result<AuthTokens>>, ITransactionalRequest;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().MaximumLength(320);
        RuleFor(command => command.Password).NotEmpty().MaximumLength(128);
    }
}

public sealed class LoginHandler(
    IUserStore users,
    IPasswordHasher passwords,
    SessionIssuer sessions,
    ILogger<LoginHandler> logger)
    : IRequestHandler<LoginCommand, Result<AuthTokens>>
{
    public async ValueTask<Result<AuthTokens>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(EmailAddress.Normalize(request.Email), cancellationToken);
        if (user is null || user.Status != UserStatus.Active || !passwords.Verify(request.Password, user.PasswordHash))
            return Result<AuthTokens>.Failure(HttpStatus.Unauthorized, "invalid_credentials", "Invalid credentials", "The email or password is incorrect.");

        logger.LogInformation("User {UserId} signed in", user.Id);
        return Result<AuthTokens>.Ok(sessions.Issue(user));
    }
}
