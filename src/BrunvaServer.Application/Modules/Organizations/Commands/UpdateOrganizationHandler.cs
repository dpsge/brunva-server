using FluentValidation;
using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Enums;
using BrunvaServer.Domain.Membership;

namespace BrunvaServer.Application.Modules.Organizations.Commands;

public sealed record UpdateOrganizationCommand(
    Guid OrganizationId,
    string Name,
    string? LegalName,
    string? TaxIdentificationNumber,
    BusinessType BusinessType,
    string DefaultCurrency,
    string TimeZone) : IRequest<Result<OrganizationResponse>>, ITransactionalRequest;

public sealed class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.LegalName).MaximumLength(300);
        RuleFor(command => command.TaxIdentificationNumber).MaximumLength(32);
        RuleFor(command => command.BusinessType).IsInEnum();
        RuleFor(command => command.DefaultCurrency).NotEmpty().Length(3);
        RuleFor(command => command.TimeZone).NotEmpty().MaximumLength(64);
    }
}

public sealed class UpdateOrganizationHandler(IOrganizationContext organizationContext, IOrganizationStore organizations)
    : IRequestHandler<UpdateOrganizationCommand, Result<OrganizationResponse>>
{
    public async ValueTask<Result<OrganizationResponse>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!organizationContext.IsSet || organizationContext.OrganizationId != request.OrganizationId)
            return Result<OrganizationResponse>.Failure(HttpStatus.Forbidden, "not_a_member", "Forbidden", "You do not have access to this organization.");

        if (!MembershipRules.CanManageOrganization(organizationContext.Role))
            return Result<OrganizationResponse>.Failure(HttpStatus.Forbidden, "forbidden", "Forbidden", "You cannot change this organization.");

        var organization = await organizations.FindAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
            return Result<OrganizationResponse>.Failure(HttpStatus.NotFound, "organization_not_found", "Not found", "The organization was not found.");

        if (!OrganizationInput.TryNormalize(request.DefaultCurrency, request.TimeZone, out var currency, out var timeZone, out var error))
            return Result<OrganizationResponse>.Failure(HttpStatus.BadRequest, "invalid_organization", "Invalid organization", error);

        organization.Update(
            request.Name.Trim(),
            TextValues.BlankToNull(request.LegalName),
            TextValues.BlankToNull(request.TaxIdentificationNumber),
            request.BusinessType,
            currency,
            timeZone);
        return Result<OrganizationResponse>.Ok(OrganizationInput.ToResponse(organization, organizationContext.Role));
    }
}
