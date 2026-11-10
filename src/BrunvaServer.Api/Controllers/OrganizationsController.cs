using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mediator;
using BrunvaServer.Application.Modules.Organizations.Commands;
using BrunvaServer.Application.Modules.Organizations.Queries;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/organizations")]
public sealed class OrganizationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ListOrganizationsQuery(page, pageSize), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrganizationCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> Get(Guid organizationId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetOrganizationQuery(organizationId), cancellationToken));

    [HttpPatch("{organizationId:guid}")]
    public async Task<IActionResult> Update(Guid organizationId, UpdateOrganizationBody body, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(
            new UpdateOrganizationCommand(
                organizationId,
                body.Name,
                body.LegalName,
                body.TaxIdentificationNumber,
                body.BusinessType,
                body.DefaultCurrency,
                body.TimeZone),
            cancellationToken));
}

public sealed record UpdateOrganizationBody(
    string Name,
    string? LegalName,
    string? TaxIdentificationNumber,
    BusinessType BusinessType,
    string DefaultCurrency,
    string TimeZone);
