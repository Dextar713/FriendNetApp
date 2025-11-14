using System.ComponentModel.DataAnnotations;

namespace FriendNetApp.UserProfile.Models
{
    public class AppUser
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required] [MaxLength(50)] 
        public required string UserName { get; set; }

        [Required] [MaxLength(70)] [EmailAddress]
        public required string Email { get; set; }

        public string ProfileImageUrl { get; set; } = string.Empty;

        public int? Age { get; set; }

        public Guid? PhotoId { get; set; }
        public Photo? Photo { get; set; }
    }
}
