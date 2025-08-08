using CopyHere.Core.Entity;
using CopyHere.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CopyHere.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClipboardEntry> ClipboardEntries { get; set; } = default!;
        public DbSet<Device> Devices { get; set; } = default!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important for IdentityDbContext

            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure relationships if not already handled by conventions or configurations
            modelBuilder.Entity<User>()
                .HasMany(u => u.Devices)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete devices when user is deleted

            modelBuilder.Entity<User>()
                .HasMany(u => u.ClipboardEntries)
                .WithOne(ce => ce.User)
                .HasForeignKey(ce => ce.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete clipboard entries when user is deleted

            modelBuilder.Entity<Device>()
                .HasMany(d => d.ClipboardEntries)
                .WithOne(ce => ce.Device)
                .HasForeignKey(ce => ce.DeviceId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting device if it has clipboard entries

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
