namespace FriendNetApp.AuthService.Models
{
    public class UserDto
    {
        public required string Email { get; set; }

        public required string Password { get; set; }

        public string Role { get; set; } = "Client";
    }
}
