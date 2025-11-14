using System.Text.Json.Serialization;

namespace FriendNetApp.UserProfile.Models
{
    public class Photo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Url { get; set; }
        public required string PublicId { get; set; }
        public Guid? UserId { get; set; }
        [JsonIgnore]
        public AppUser? User { get; set; }
    }
}
