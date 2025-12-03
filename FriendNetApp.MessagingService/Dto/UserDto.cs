namespace FriendNetApp.MessagingService.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
    }
}
