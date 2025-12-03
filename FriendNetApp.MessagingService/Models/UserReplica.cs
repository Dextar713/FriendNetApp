using System.ComponentModel.DataAnnotations;

namespace FriendNetApp.MessagingService.Models
{
    public class UserReplica
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(70)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = null!;

        public string ProfileImageUrl { get; set; } = string.Empty;

        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();

    }
}
