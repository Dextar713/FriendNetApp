namespace FriendNetApp.MessagingService.Models
{
    public class Chat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }

        public UserReplica? User1 { get; set; }
        public UserReplica? User2 { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
 