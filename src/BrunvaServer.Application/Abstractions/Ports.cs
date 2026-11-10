using BrunvaServer.Application.Common;
using BrunvaServer.Domain.Entities;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface ICurrentUser
{
    Guid? UserId { get; }
}

public interface IOrganizationContext
{
    bool IsSet { get; }
    Guid OrganizationId { get; }
    MemberRole Role { get; }
    void Set(Guid organizationId, MemberRole role);
}

public interface IUnitOfWork
{
    Task BeginAsync(CancellationToken cancellationToken);
    Task CommitAsync(CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public interface IAccessTokenIssuer
{
    AccessToken Issue(User user, DateTimeOffset now);
}

public interface IRefreshTokenFactory
{
    string Create();
    string Hash(string token);
}

public sealed class AuthSettings
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = "brunva";

    public string Audience { get; set; } = "brunva";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 14;
}

public interface IUserStore
{
    void Add(User user);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}

public interface IRefreshTokenStore
{
    void Add(RefreshToken token);
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task RevokeActiveAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);
}

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    string? LegalName,
    string? TaxIdentificationNumber,
    BusinessType BusinessType,
    string DefaultCurrency,
    string TimeZone,
    OrganizationStatus Status,
    MemberRole Role);

public interface IOrganizationStore
{
    void Add(Organization organization);
    Task<Organization?> FindAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<Page<OrganizationResponse>> ListForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}

public sealed record MemberResponse(
    Guid Id,
    Guid UserId,
    string Email,
    string FullName,
    MemberRole Role,
    MemberStatus Status);

public interface IMembershipStore
{
    void Add(OrganizationMember member);
    void Remove(OrganizationMember member);
    Task<OrganizationMember?> FindByUserAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);
    Task<OrganizationMember?> FindAsync(Guid organizationId, Guid memberId, CancellationToken cancellationToken);
    Task<Page<MemberResponse>> ListAsync(Guid organizationId, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountActiveOwnersAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);
}
