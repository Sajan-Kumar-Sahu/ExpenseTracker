using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Persistence.Configurations
{
    public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.ToTable("Reminders");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.Property(x => x.ReminderType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ReferenceModule)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(ReminderStatus.Pending);

            builder.Property(x => x.RepeatType)
                .HasConversion<int>()
                .HasDefaultValue(RepeatType.None);

            builder.Property(x => x.IsPushNotificationEnabled).HasDefaultValue(false);
            builder.Property(x => x.IsInAppNotificationEnabled).HasDefaultValue(true);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Reminders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.ScheduledDate);
            builder.HasIndex(x => x.ReminderGroupId);
            builder.HasIndex(x => new { x.UserId, x.Status });
            builder.HasIndex(x => new { x.ReferenceModule, x.ReferenceId });
            builder.HasIndex(x => new { x.UserId, x.ReminderType, x.ReferenceModule, x.ReferenceId });
        }
    }
}
