using Microsoft.EntityFrameworkCore;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Entities;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Infrastructure.Persistence;

public sealed class UserStore(BrunvaDbContext db) : IUserStore
{
    public void Add(User user) => db.Users.Add(user);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
}

public sealed class RefreshTokenStore(BrunvaDbContext db) : IRefreshTokenStore
{
    public void Add(RefreshToken token) => db.RefreshTokens.Add(token);

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        db.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public async Task RevokeActiveAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var tokens = await db.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
            token.Revoke(now);
    }
}

public sealed class OrganizationStore(BrunvaDbContext db) : IOrganizationStore
{
    public void Add(Organization organization) => db.Organizations.Add(organization);

    public Task<Organization?> FindAsync(Guid organizationId, CancellationToken cancellationToken) =>
        db.Organizations.SingleOrDefaultAsync(organization => organization.Id == organizationId, cancellationToken);

    public async Task<Page<OrganizationResponse>> ListForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedSize) = PageWindow.Normalize(page, pageSize);
        var query =
            from member in db.OrganizationMembers.AsNoTracking()
            join organization in db.Organizations.AsNoTracking() on member.OrganizationId equals organization.Id
            where member.UserId == userId && member.Status == MemberStatus.Active && organization.Status == OrganizationStatus.Active
            orderby organization.Name
            select new OrganizationResponse(
                organization.Id,
                organization.Name,
                organization.LegalName,
                organization.TaxIdentificationNumber,
                organization.BusinessType,
                organization.DefaultCurrency,
                organization.TimeZone,
                organization.Status,
                member.Role);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((normalizedPage - 1) * normalizedSize).Take(normalizedSize).ToListAsync(cancellationToken);
        return new Page<OrganizationResponse>(items, normalizedPage, normalizedSize, total);
    }
}

public sealed class MembershipStore(BrunvaDbContext db) : IMembershipStore
{
    public void Add(OrganizationMember member) => db.OrganizationMembers.Add(member);

    public void Remove(OrganizationMember member) => db.OrganizationMembers.Remove(member);

    public Task<OrganizationMember?> FindByUserAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken) =>
        db.OrganizationMembers.SingleOrDefaultAsync(member => member.OrganizationId == organizationId && member.UserId == userId, cancellationToken);

    public Task<OrganizationMember?> FindAsync(Guid organizationId, Guid memberId, CancellationToken cancellationToken) =>
        db.OrganizationMembers.SingleOrDefaultAsync(member => member.OrganizationId == organizationId && member.Id == memberId, cancellationToken);

    public Task<bool> ExistsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken) =>
        db.OrganizationMembers.AnyAsync(member => member.OrganizationId == organizationId && member.UserId == userId, cancellationToken);

    public Task<int> CountActiveOwnersAsync(Guid organizationId, CancellationToken cancellationToken) =>
        db.OrganizationMembers.CountAsync(
            member => member.OrganizationId == organizationId && member.Role == MemberRole.Owner && member.Status == MemberStatus.Active,
            cancellationToken);

    public async Task<Page<MemberResponse>> ListAsync(
        Guid organizationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedSize) = PageWindow.Normalize(page, pageSize);
        var query =
            from member in db.OrganizationMembers.AsNoTracking()
            join user in db.Users.AsNoTracking() on member.UserId equals user.Id
            where member.OrganizationId == organizationId
            orderby user.FullName
            select new MemberResponse(member.Id, user.Id, user.Email, user.FullName, member.Role, member.Status);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((normalizedPage - 1) * normalizedSize).Take(normalizedSize).ToListAsync(cancellationToken);
        return new Page<MemberResponse>(items, normalizedPage, normalizedSize, total);
    }
}
