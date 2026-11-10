namespace BrunvaServer.Application.Common;

public static class TextValues
{
    public static string? BlankToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}

public static class EmailAddress
{
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
