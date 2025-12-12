namespace FriendNetApp.SocialService.Dto
{
    public class FriendMatchRequestDto
    {
        public required Guid UserAId { get; set; }
        public required Guid UserBId { get; set; }
        public Guid? InviterId { get; set; }
    }
}
