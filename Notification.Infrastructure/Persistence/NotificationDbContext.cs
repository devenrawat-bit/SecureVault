using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence.Entities;

namespace Notification.Infrastructure.Persistence;

public partial class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EmailDelivery> EmailDeliveries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailDelivery>(entity =>
        {
            entity.HasIndex(e => e.MessageId, "UQ_EmailDeliveries_MessageId").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_EmailDeliveries_CreatedAtUtc");
            entity.Property(e => e.LastError).HasMaxLength(1000);
            entity.Property(e => e.MessageId).HasMaxLength(200);
            entity.Property(e => e.NotificationType).HasMaxLength(50);
            entity.Property(e => e.RecipientEmail).HasMaxLength(320);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending", "DF_EmailDeliveries_Status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
