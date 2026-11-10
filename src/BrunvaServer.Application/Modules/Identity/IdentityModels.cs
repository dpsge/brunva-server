using Microsoft.Extensions.Options;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Domain.Entities;

namespace BrunvaServer.Application.Modules.Identity;

public sealed record UserProfile(Guid Id, string Email, string FullName, string? PhoneNumber);

public sealed record AuthTokens(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    UserProfile User);

public static class UserMapper
{
    public static UserProfile ToProfile(User user) =>
        new(user.Id, user.Email, user.FullName, user.PhoneNumber);
}

public sealed class SessionIssuer(
    IAccessTokenIssuer accessTokens,
    IRefreshTokenFactory refreshTokens,
    IRefreshTokenStore store,
    IClock clock,
    IOptions<AuthSettings> settings)
{
    public AuthTokens Issue(User user)
    {
        var now = clock.UtcNow;
        var raw = refreshTokens.Create();
        var token = RefreshToken.Issue(user.Id, refreshTokens.Hash(raw), now.AddDays(settings.Value.RefreshTokenDays));
        store.Add(token);
        var access = accessTokens.Issue(user, now);
        return new AuthTokens(access.Value, access.ExpiresAt, raw, UserMapper.ToProfile(user));
    }

    public AuthTokens Rotate(User user, RefreshToken current)
    {
        var now = clock.UtcNow;
        var raw = refreshTokens.Create();
        var token = RefreshToken.Issue(user.Id, refreshTokens.Hash(raw), now.AddDays(settings.Value.RefreshTokenDays));
        current.Revoke(now, token.Id);
        store.Add(token);
        var access = accessTokens.Issue(user, now);
        return new AuthTokens(access.Value, access.ExpiresAt, raw, UserMapper.ToProfile(user));
    }
}
