using FluentValidation;
using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Entities;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Application.Modules.Organizations.Commands;

public sealed record CreateOrganizationCommand(
    string Name,
    string? LegalName,
    string? TaxIdentificationNumber,
    BusinessType BusinessType,
    string? DefaultCurrency,
    string? TimeZone) : IRequest<Result<OrganizationResponse>>, ITransactionalRequest;

public sealed class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.LegalName).MaximumLength(300);
        RuleFor(command => command.TaxIdentificationNumber).MaximumLength(32);
        RuleFor(command => command.BusinessType).IsInEnum();
        RuleFor(command => command.DefaultCurrency).MaximumLength(3);
        RuleFor(command => command.TimeZone).MaximumLength(64);
    }
}

public sealed class CreateOrganizationHandler(
    ICurrentUser currentUser,
    IOrganizationStore organizations,
    IMembershipStore memberships,
    ILogger<CreateOrganizationHandler> logger)
    : IRequestHandler<CreateOrganizationCommand, Result<OrganizationResponse>>
{
    public async ValueTask<Result<OrganizationResponse>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
            return Result<OrganizationResponse>.Failure(HttpStatus.Unauthorized, "unauthenticated", "Unauthenticated", "Sign in is required.");

        if (!OrganizationInput.TryNormalize(request.DefaultCurrency, request.TimeZone, out var currency, out var timeZone, out var error))
            return Result<OrganizationResponse>.Failure(HttpStatus.BadRequest, "invalid_organization", "Invalid organization", error);

        var organization = Organization.Create(
            request.Name.Trim(),
            TextValues.BlankToNull(request.LegalName),
            TextValues.BlankToNull(request.TaxIdentificationNumber),
            request.BusinessType,
            currency,
            timeZone);
        organizations.Add(organization);
        memberships.Add(OrganizationMember.Create(organization.Id, userId, MemberRole.Owner));
        logger.LogInformation("User {UserId} created organization {OrganizationId}", userId, organization.Id);
        return Result<OrganizationResponse>.Created(OrganizationInput.ToResponse(organization, MemberRole.Owner));
    }
}
