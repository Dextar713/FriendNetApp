using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendNetApp.IntegrationTests
{
    public class TestingDto
    {
        public class AuthUserDto
        {
            public required string Email { get; set; }

            public required string Password { get; set; }

            public string Role { get; set; } = "Client";
        }

        // DTO for UsersController
        public class UserProfileInputDto
        {
            public required string UserName { get; set; }
            public required string Email { get; set; }

            public int? Age;
        }

        // DTO for deserializing the User Profile response
        public class UserProfileOutputDto
        {
            public required string UserName { get; set; }
            public required string Email { get; set; }

            public int? Age { get; set; }
        }
    }
}
