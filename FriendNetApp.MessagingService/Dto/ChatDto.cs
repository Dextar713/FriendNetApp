using FriendNetApp.MessagingService.Models;

namespace FriendNetApp.MessagingService.Dto
{
    public class ChatDto
    {
        public Guid Id { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.Now;

        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();

    }
}
