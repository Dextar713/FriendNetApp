using System.ComponentModel.DataAnnotations;

namespace FriendNetApp.MessagingService.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }

        [Required] [MaxLength(700)]
        public required string Content { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public Chat? Chat { get; set; }
        public UserReplica? Sender { get; set; }
    }
}
 