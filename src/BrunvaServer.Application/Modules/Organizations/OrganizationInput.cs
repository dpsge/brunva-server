using BrunvaServer.Application.Abstractions;
using BrunvaServer.Domain.Entities;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Application.Modules.Organizations;

public static class OrganizationInput
{
    public static bool TryNormalize(
        string? currency,
        string? timeZone,
        out string normalizedCurrency,
        out string normalizedTimeZone,
        out string error)
    {
        var currencyValue = string.IsNullOrWhiteSpace(currency) ? "GEL" : currency.Trim().ToUpperInvariant();
        if (currencyValue.Length != 3 || currencyValue.Any(character => !char.IsLetter(character)))
        {
            normalizedCurrency = string.Empty;
            normalizedTimeZone = string.Empty;
            error = "Currency must be a three-letter code.";
            return false;
        }

        var zoneValue = string.IsNullOrWhiteSpace(timeZone) ? "Asia/Tbilisi" : timeZone.Trim();
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(zoneValue, out _))
        {
            normalizedCurrency = string.Empty;
            normalizedTimeZone = string.Empty;
            error = "Time zone is not recognized.";
            return false;
        }

        normalizedCurrency = currencyValue;
        normalizedTimeZone = zoneValue;
        error = string.Empty;
        return true;
    }

    public static OrganizationResponse ToResponse(Organization organization, MemberRole role) =>
        new(
            organization.Id,
            organization.Name,
            organization.LegalName,
            organization.TaxIdentificationNumber,
            organization.BusinessType,
            organization.DefaultCurrency,
            organization.TimeZone,
            organization.Status,
            role);
}
