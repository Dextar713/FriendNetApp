using System.ComponentModel.DataAnnotations;

namespace FriendNetApp.SocialService.Models
{
    public class Match
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public MatchType Type { get; set; }

        [Required]
        public required Guid User1Id { get; set; }
        [Required]
        public required Guid User2Id { get; set; }

        public Guid? InviterId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? Rating1 { get; set; }
        public int? Rating2 { get; set; }
        public UserNode? User1 { get; set; }
        public UserNode? User2 { get; set; }
        public UserNode? Inviter { get; set; }
    }
}
