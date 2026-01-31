namespace FriendNetApp.UserProfile.Dto
{
    public class UserInputDto
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }

        public int? Age { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
