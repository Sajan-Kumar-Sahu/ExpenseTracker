using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ExpenseTracker.Persistence.Context
{
    public class ExpenseTrackerDbContext : DbContext
    {
        public ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Account> Accounts => Set<Account>();

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();

        public DbSet<Contact> Contacts => Set<Contact>();

        public DbSet<Settlement> Settlements => Set<Settlement>();

        public DbSet<Project> Projects => Set<Project>();

        public DbSet<WorkLog> WorkLogs => Set<WorkLog>();

        public DbSet<Reminder> Reminders => Set<Reminder>();

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpenseTrackerDbContext).Assembly);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ExpenseTrackerDbContext)
                        .GetMethod(nameof(SetSoftDeleteFilter),
                            BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder });
                }
            }
        }
        private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : AuditableEntity
        {
            builder.Entity<TEntity>()
                .HasQueryFilter(x => !x.IsDeleted);
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries<AuditableEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
