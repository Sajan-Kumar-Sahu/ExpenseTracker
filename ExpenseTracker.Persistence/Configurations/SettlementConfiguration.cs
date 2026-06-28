using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Persistence.Configurations
{
    public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
    {
        public void Configure(EntityTypeBuilder<Settlement> builder)
        {
            builder.ToTable("Settlements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SettlementType)
                .HasConversion<int>();

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.OriginalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.PendingAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasConversion<int>();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Settlements)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Contact)
                .WithMany(x => x.Settlements)
                .HasForeignKey(x => x.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ContactId);
            builder.HasIndex(x => new { x.UserId, x.Status });
        }
    }
}
