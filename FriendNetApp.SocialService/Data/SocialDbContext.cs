using FriendNetApp.SocialService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.Data
{
    public class SocialDbContext(DbContextOptions<SocialDbContext> options): DbContext(options)
    {
        public DbSet<Match> Matches { get; set; }
        public DbSet<UserNode> UserNodes { get; set; } 
        public DbSet<Friendship> Friendships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasKey(f => new { f.User1Id, f.User2Id });
                entity.HasOne(f => f.User1)
                    .WithMany()
                    .HasForeignKey(f => f.User1Id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.User2)
                    .WithMany()
                    .HasForeignKey(f => f.User2Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Type)
                    .HasConversion<string>();

                entity.HasOne(m => m.User1)
                    .WithMany()
                    .HasForeignKey(m => m.User1Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.User2)
                    .WithMany()
                    .HasForeignKey(m => m.User2Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Inviter)
                    .WithMany()
                    .HasForeignKey(m => m.InviterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
