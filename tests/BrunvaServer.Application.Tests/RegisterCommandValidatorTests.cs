using FluentAssertions;
using BrunvaServer.Application.Modules.Identity.Commands;

namespace BrunvaServer.Application.Tests;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Rejects_a_short_password()
    {
        var result = _validator.Validate(new RegisterCommand("ada@example.com", "short1", "Ada", null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Accepts_a_password_with_a_letter_and_a_digit()
    {
        var result = _validator.Validate(new RegisterCommand("ada@example.com", "Password1", "Ada", null));

        result.IsValid.Should().BeTrue();
    }
}
