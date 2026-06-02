using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Persistence.Configurations
{
    public class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.CategoryType)
                .HasConversion<int>();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.UserId,
                x.Name,
                x.CategoryType
            }).IsUnique();
        }
    }
}
