namespace FriendNetApp.UserProfile.Dto
{
    public class UserOutputDto
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }

        public int? Age { get; set; }
    }
}
