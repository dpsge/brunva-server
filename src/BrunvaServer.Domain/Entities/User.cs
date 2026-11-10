using BrunvaServer.Domain.Common;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Domain.Entities;

public sealed class User : Entity
{
    private User()
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public UserStatus Status { get; private set; } = UserStatus.Active;

    public static User Create(string email, string passwordHash, string fullName, string? phoneNumber) =>
        new()
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            PhoneNumber = phoneNumber
        };
}
