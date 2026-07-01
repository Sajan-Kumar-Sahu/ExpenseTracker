using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.RefreshToken)
            .HasMaxLength(512);

        builder.Property(x => x.RefreshTokenExpiry);

        builder.Property(x => x.DeviceToken)
            .HasMaxLength(512);

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}