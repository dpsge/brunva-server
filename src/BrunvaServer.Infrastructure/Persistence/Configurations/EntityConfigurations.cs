using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrunvaServer.Domain.Entities;

namespace BrunvaServer.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(user => user.FullName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.PhoneNumber).HasMaxLength(32);
        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(32);
    }
}

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(organization => organization.Id);
        builder.Property(organization => organization.Name).HasMaxLength(200).IsRequired();
        builder.Property(organization => organization.LegalName).HasMaxLength(300);
        builder.Property(organization => organization.TaxIdentificationNumber).HasMaxLength(32);
        builder.HasIndex(organization => organization.TaxIdentificationNumber)
            .IsUnique()
            .HasFilter("tax_identification_number IS NOT NULL");
        builder.Property(organization => organization.BusinessType).HasConversion<string>().HasMaxLength(32);
        builder.Property(organization => organization.DefaultCurrency).HasMaxLength(3).IsRequired();
        builder.Property(organization => organization.TimeZone).HasMaxLength(64).IsRequired();
        builder.Property(organization => organization.Status).HasConversion<string>().HasMaxLength(32);
    }
}

public sealed class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.HasKey(member => member.Id);
        builder.HasIndex(member => new { member.OrganizationId, member.UserId }).IsUnique();
        builder.Property(member => member.Role).HasConversion<string>().HasMaxLength(32);
        builder.Property(member => member.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasOne<Organization>().WithMany().HasForeignKey(member => member.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(member => member.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(token => token.Id);
        builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.UserId);
        builder.HasOne<User>().WithMany().HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
