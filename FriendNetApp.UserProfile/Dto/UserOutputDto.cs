namespace FriendNetApp.UserProfile.Dto
{
    public class UserOutputDto
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }

        public int? Age { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
