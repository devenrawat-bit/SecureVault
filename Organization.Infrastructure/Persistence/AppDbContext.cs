using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Organization.Domain.Entities;

namespace Organization.Infrastructure.Persistence
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(
       DbContextOptions<AppDbContext> options)
       : base(options)
        {
        }

        public DbSet<Organizations> Organizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organizations>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(x => x.Code)
                    .IsUnique();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .IsRequired();
            });
        }
    }
}
