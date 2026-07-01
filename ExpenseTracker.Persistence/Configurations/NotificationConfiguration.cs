using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Body)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ActionUrl).HasMaxLength(1000);

            builder.Property(x => x.NotificationType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ReferenceModule)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.DeliveryStatus)
                .HasConversion<int>()
                .HasDefaultValue(DeliveryStatus.Pending);

            builder.Property(x => x.IsRead).HasDefaultValue(false);
            builder.Property(x => x.IsClicked).HasDefaultValue(false);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Reminder)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.ReminderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ReminderId);
            builder.HasIndex(x => new { x.UserId, x.IsRead });
            builder.HasIndex(x => new { x.UserId, x.SentAt });
        }
    }
}
