using BrunvaServer.Domain.Common;
using BrunvaServer.Domain.Enums;

namespace BrunvaServer.Domain.Entities;

public sealed class Organization : Entity
{
    private Organization()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string? LegalName { get; private set; }

    public string? TaxIdentificationNumber { get; private set; }

    public BusinessType BusinessType { get; private set; }

    public string DefaultCurrency { get; private set; } = "GEL";

    public string TimeZone { get; private set; } = "Asia/Tbilisi";

    public OrganizationStatus Status { get; private set; } = OrganizationStatus.Active;

    public static Organization Create(
        string name,
        string? legalName,
        string? taxIdentificationNumber,
        BusinessType businessType,
        string defaultCurrency,
        string timeZone) =>
        new()
        {
            Name = name,
            LegalName = legalName,
            TaxIdentificationNumber = taxIdentificationNumber,
            BusinessType = businessType,
            DefaultCurrency = defaultCurrency,
            TimeZone = timeZone
        };

    public void Update(
        string name,
        string? legalName,
        string? taxIdentificationNumber,
        BusinessType businessType,
        string defaultCurrency,
        string timeZone)
    {
        Name = name;
        LegalName = legalName;
        TaxIdentificationNumber = taxIdentificationNumber;
        BusinessType = businessType;
        DefaultCurrency = defaultCurrency;
        TimeZone = timeZone;
    }
}
