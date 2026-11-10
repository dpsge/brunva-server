using BrunvaServer.Domain.Common;

namespace BrunvaServer.Domain.Entities;

public sealed class RefreshToken : Entity
{
    private RefreshToken()
    {
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public static RefreshToken Issue(Guid userId, string tokenHash, DateTimeOffset expiresAt) =>
        new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        };

    public void Revoke(DateTimeOffset now, Guid? replacedByTokenId = null)
    {
        RevokedAt = now;
        ReplacedByTokenId = replacedByTokenId;
    }
}
