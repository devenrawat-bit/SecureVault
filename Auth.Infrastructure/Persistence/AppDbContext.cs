using Auth.Domain.Entities;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    //here above we are telling that the applcationuser is the user type, role type is the identity role<guid> and the key type is guid. there are identityUser and the identityRole two things 
    {
        public AppDbContext(
       DbContextOptions<AppDbContext> options)
       : base(options)
        {
        }
        public DbSet<Invitation> Invitations { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Invitation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(x => x.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.TokenHash)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique();
            });
        }
    }
}
