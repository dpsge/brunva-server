using FluentValidation;
using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Entities;

namespace BrunvaServer.Application.Modules.Identity.Commands;

public sealed record RegisterCommand(string Email, string Password, string FullName, string? PhoneNumber)
    : IRequest<Result<UserProfile>>, ITransactionalRequest;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Za-z]").WithMessage("Password must include a letter.")
            .Matches("[0-9]").WithMessage("Password must include a digit.");
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.PhoneNumber).MaximumLength(32);
    }
}

public sealed class RegisterHandler(IUserStore users, IPasswordHasher passwords, ILogger<RegisterHandler> logger)
    : IRequestHandler<RegisterCommand, Result<UserProfile>>
{
    public async ValueTask<Result<UserProfile>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Normalize(request.Email);
        if (await users.FindByEmailAsync(email, cancellationToken) is not null)
            return Result<UserProfile>.Failure(HttpStatus.Conflict, "email_taken", "Email is already registered", "Use a different email address.");

        var user = User.Create(email, passwords.Hash(request.Password), request.FullName.Trim(), TextValues.BlankToNull(request.PhoneNumber));
        users.Add(user);
        logger.LogInformation("User {UserId} registered", user.Id);
        return Result<UserProfile>.Created(UserMapper.ToProfile(user));
    }
}
