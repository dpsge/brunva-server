namespace BrunvaServer.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Disabled = 2
}

public enum OrganizationStatus
{
    Active = 1,
    Suspended = 2
}

public enum MemberRole
{
    Owner = 1,
    Admin = 2,
    Accountant = 3,
    Operator = 4,
    Viewer = 5
}

public enum MemberStatus
{
    Active = 1,
    Suspended = 2
}

public enum BusinessType
{
    IndividualEntrepreneur = 1,
    LegalEntity = 2
}
