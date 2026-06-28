using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Persistence.Configurations
{
    public class WorkLogConfiguration : IEntityTypeConfiguration<WorkLog>
    {
        public void Configure(EntityTypeBuilder<WorkLog> builder)
        {
            builder.ToTable("WorkLogs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.WorkDate).IsRequired();
            builder.Property(x => x.WorkType).HasConversion<int>().IsRequired();
            builder.Property(x => x.StartTime).IsRequired();
            builder.Property(x => x.EndTime).IsRequired();
            builder.Property(x => x.WorkedHours).HasPrecision(5, 2).IsRequired();

            builder.Property(x => x.TaskTitle)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
            builder.Property(x => x.ActualAmount).HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(WorkLogStatus.Draft);

            builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
            builder.Property(x => x.PaymentMonth).HasMaxLength(50);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.User)
                .WithMany(x => x.WorkLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.WorkLogs)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ProjectId);
            builder.HasIndex(x => new { x.UserId, x.WorkDate });
            builder.HasIndex(x => new { x.UserId, x.Status });
        }
    }
}
