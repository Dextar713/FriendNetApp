namespace FriendNetApp.SocialService.Dto
{
    public class CreateFriendshipRequestDto
    {
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
    }
}
