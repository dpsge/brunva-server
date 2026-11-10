using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mediator;
using BrunvaServer.Application.Modules.Organizations.Commands;
using BrunvaServer.Application.Modules.Organizations.Queries;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/members")]
public sealed class MembersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid organizationId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ListMembersQuery(organizationId, page, pageSize), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Add(Guid organizationId, AddMemberBody body, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new AddMemberCommand(organizationId, body.Email, body.Role), cancellationToken));

    [HttpPatch("{memberId:guid}")]
    public async Task<IActionResult> Update(Guid organizationId, Guid memberId, UpdateMemberBody body, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpdateMemberCommand(organizationId, memberId, body.Role, body.Status), cancellationToken));

    [HttpDelete("{memberId:guid}")]
    public async Task<IActionResult> Remove(Guid organizationId, Guid memberId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new RemoveMemberCommand(organizationId, memberId), cancellationToken));
}

public sealed record AddMemberBody(string Email, MemberRole Role);

public sealed record UpdateMemberBody(MemberRole Role, MemberStatus Status);
