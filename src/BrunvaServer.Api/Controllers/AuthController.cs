using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mediator;
using BrunvaServer.Application.Modules.Identity;
using BrunvaServer.Application.Modules.Identity.Commands;
using BrunvaServer.Application.Modules.Identity.Queries;

namespace BrunvaServer.Api.Controllers;

[ApiController]
[EnableRateLimiting("auth")]
[Route("api/v1/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetMeQuery(), cancellationToken));
}
