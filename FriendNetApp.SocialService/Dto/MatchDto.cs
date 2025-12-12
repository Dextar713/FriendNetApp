namespace FriendNetApp.SocialService.Dto
{
    public class MatchDto
    {
        public required Guid Id { get; set; }
        public MatchType Type { get; set; }
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        public Guid? InviterId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
