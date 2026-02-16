using FriendNetApp.MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.MessagingService.Data
{
    public class MessagingDbContext(DbContextOptions<MessagingDbContext> options) : DbContext(options)
    {
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserReplica> UserReplicas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== USERREPLICA ==========
            modelBuilder.Entity<UserReplica>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<UserReplica>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                // When a user replica is deleted we want dependents to be removed to avoid FK violations
                .OnDelete(DeleteBehavior.Cascade);

            // ========== CHAT ==========
            modelBuilder.Entity<Chat>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                // Remove chats when referenced user is deleted
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                // Remove chats when referenced user is deleted
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== MESSAGE ==========
            modelBuilder.Entity<Message>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                // Keep messages removed when sender replica is deleted
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
