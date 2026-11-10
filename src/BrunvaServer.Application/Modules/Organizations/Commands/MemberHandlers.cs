using FluentValidation;
using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Entities;
using BrunvaServer.Domain.Enums;
using BrunvaServer.Domain.Membership;

namespace BrunvaServer.Application.Modules.Organizations.Commands;

public sealed record AddMemberCommand(Guid OrganizationId, string Email, MemberRole Role)
    : IRequest<Result<MemberResponse>>, ITransactionalRequest;

public sealed class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Role).IsInEnum();
    }
}

public sealed class AddMemberHandler(
    IOrganizationContext organizationContext,
    IUserStore users,
    IMembershipStore memberships)
    : IRequestHandler<AddMemberCommand, Result<MemberResponse>>
{
    public async ValueTask<Result<MemberResponse>> Handle(AddMemberCommand request, CancellationToken cancellationToken)
    {
        if (!organizationContext.IsSet || organizationContext.OrganizationId != request.OrganizationId)
            return Forbidden("not_a_member", "You do not have access to this organization.");

        if (!MembershipRules.CanAssign(organizationContext.Role, request.Role))
            return Forbidden("forbidden", "You cannot assign this role.");

        var user = await users.FindByEmailAsync(EmailAddress.Normalize(request.Email), cancellationToken);
        if (user is null)
            return Result<MemberResponse>.Failure(HttpStatus.NotFound, "user_not_found", "Not found", "No user is registered with this email.");

        if (await memberships.ExistsAsync(request.OrganizationId, user.Id, cancellationToken))
            return Result<MemberResponse>.Failure(HttpStatus.Conflict, "already_member", "Already a member", "This user is already a member.");

        var member = OrganizationMember.Create(request.OrganizationId, user.Id, request.Role);
        memberships.Add(member);
        return Result<MemberResponse>.Created(new MemberResponse(member.Id, user.Id, user.Email, user.FullName, member.Role, member.Status));
    }

    private static Result<MemberResponse> Forbidden(string code, string detail) =>
        Result<MemberResponse>.Failure(HttpStatus.Forbidden, code, "Forbidden", detail);
}

public sealed record UpdateMemberCommand(Guid OrganizationId, Guid MemberId, MemberRole Role, MemberStatus Status)
    : IRequest<Result<MemberResponse>>, ITransactionalRequest;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(command => command.Role).IsInEnum();
        RuleFor(command => command.Status).IsInEnum();
    }
}

public sealed class UpdateMemberHandler(IOrganizationContext organizationContext, IMembershipStore memberships, IUserStore users)
    : IRequestHandler<UpdateMemberCommand, Result<MemberResponse>>
{
    public async ValueTask<Result<MemberResponse>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        if (!organizationContext.IsSet || organizationContext.OrganizationId != request.OrganizationId)
            return Forbidden("not_a_member", "You do not have access to this organization.");

        var member = await memberships.FindAsync(request.OrganizationId, request.MemberId, cancellationToken);
        if (member is null)
            return Result<MemberResponse>.Failure(HttpStatus.NotFound, "member_not_found", "Not found", "The member was not found.");

        if (!MembershipRules.CanChange(organizationContext.Role, member.Role, request.Role))
            return Forbidden("forbidden", "You cannot change this member.");

        var remainsOwner = request.Role == MemberRole.Owner && request.Status == MemberStatus.Active;
        var owners = await memberships.CountActiveOwnersAsync(request.OrganizationId, cancellationToken);
        if (MembershipRules.RemovesLastOwner(member.Role, remainsOwner, owners))
            return Result<MemberResponse>.Failure(HttpStatus.Conflict, "last_owner", "Last owner", "The organization must keep an active owner.");

        member.Change(request.Role, request.Status);
        var user = await users.FindByIdAsync(member.UserId, cancellationToken);
        return Result<MemberResponse>.Ok(new MemberResponse(
            member.Id,
            member.UserId,
            user?.Email ?? string.Empty,
            user?.FullName ?? string.Empty,
            member.Role,
            member.Status));
    }

    private static Result<MemberResponse> Forbidden(string code, string detail) =>
        Result<MemberResponse>.Failure(HttpStatus.Forbidden, code, "Forbidden", detail);
}

public sealed record RemoveMemberCommand(Guid OrganizationId, Guid MemberId)
    : IRequest<Result<EmptyBody>>, ITransactionalRequest;

public sealed class RemoveMemberHandler(IOrganizationContext organizationContext, IMembershipStore memberships)
    : IRequestHandler<RemoveMemberCommand, Result<EmptyBody>>
{
    public async ValueTask<Result<EmptyBody>> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        if (!organizationContext.IsSet || organizationContext.OrganizationId != request.OrganizationId)
            return Result<EmptyBody>.Failure(HttpStatus.Forbidden, "not_a_member", "Forbidden", "You do not have access to this organization.");

        var member = await memberships.FindAsync(request.OrganizationId, request.MemberId, cancellationToken);
        if (member is null)
            return Result<EmptyBody>.Failure(HttpStatus.NotFound, "member_not_found", "Not found", "The member was not found.");

        if (!MembershipRules.CanAssign(organizationContext.Role, member.Role))
            return Result<EmptyBody>.Failure(HttpStatus.Forbidden, "forbidden", "Forbidden", "You cannot remove this member.");

        var owners = await memberships.CountActiveOwnersAsync(request.OrganizationId, cancellationToken);
        if (MembershipRules.RemovesLastOwner(member.Role, false, owners))
            return Result<EmptyBody>.Failure(HttpStatus.Conflict, "last_owner", "Last owner", "The organization must keep an active owner.");

        memberships.Remove(member);
        return Result<EmptyBody>.NoContent();
    }
}
