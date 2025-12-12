using System.ComponentModel.DataAnnotations;

namespace FriendNetApp.SocialService.Models
{
    public class Friendship
    {
        [Required]
        public required Guid User1Id { get; set; }
        [Required]
        public required Guid User2Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public UserNode? User1 { get; set; }
        public UserNode? User2 { get; set; }
    }
}
