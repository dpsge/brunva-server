using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Domain.Entities;

namespace BrunvaServer.Infrastructure.Authentication;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(new object(), passwordHash, password) != PasswordVerificationResult.Failed;
}

public sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    public string Create() => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));

    public string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

public sealed class JwtAccessTokenIssuer(IOptions<AuthSettings> options) : IAccessTokenIssuer
{
    public AccessToken Issue(User user, DateTimeOffset now)
    {
        var settings = options.Value;
        var expires = now.AddMinutes(settings.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            [
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("name", user.FullName)
            ],
            now.UtcDateTime,
            expires.UtcDateTime,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
