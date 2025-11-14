namespace FriendNetApp.UserProfile.Dto
{
    public class PhotoDto
    {
        public Guid Id { get; set; }
        public required string Url { get; set; }
        public required string PublicId { get; set; }
        public Guid? UserId { get; set; }
    }
}
