using FriendNetApp.UserProfile.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.Data
{
    public class UserProfileDbContext(DbContextOptions<UserProfileDbContext> options): DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Photo)
                .WithOne(p => p.User)
                .HasForeignKey<AppUser>(u => u.PhotoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Photo>()
                .HasOne(p => p.User)
                .WithOne(u => u.Photo)
                .HasForeignKey<Photo>(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

}
