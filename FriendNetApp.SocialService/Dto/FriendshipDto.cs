namespace FriendNetApp.SocialService.Dto
{
    public class FriendshipDto
    {
        public required Guid User1Id { get; set; } 
        public required Guid User2Id { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}
