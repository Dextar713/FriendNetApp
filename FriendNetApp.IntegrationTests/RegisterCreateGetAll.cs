using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using System.Net.Http.Headers;

namespace FriendNetApp.IntegrationTests.Tests
{
    [Collection("UserBasic")]
    public class RegisterCreateGetAll
    {
        private readonly AspireAppFixture _fixture;
        private readonly ITestOutputHelper _outputHelper; 

        public RegisterCreateGetAll(AspireAppFixture fixture, ITestOutputHelper helper)
        {
            _fixture = fixture;
            _outputHelper = helper;
        }

        private async Task<(TestingDto.AuthUserDto Auth, TestingDto.UserProfileOutputDto Profile, string Id)>
            RegisterAndCreateUserAsync(string userName, string role = "Admin")
        {
            var uniqueEmail = $"test-{Guid.NewGuid()}@friendnet.com";
            var password = "StrongPassword123!";

            var authDto = new TestingDto.AuthUserDto
            {
                Email = uniqueEmail,
                Password = password,
                Role = role
            };

            //1. Register (this also logs us in and sets the cookie)
            var authResponse = await _fixture.GatewayClient.PostAsJsonAsync("/friendnet/auth/register", authDto);
            authResponse.EnsureSuccessStatusCode(); // Fail fast if register fails

            //2. Create Profile
            var profileDto = new TestingDto.UserProfileInputDto
            {
                UserName = userName,
                Email = uniqueEmail
            };
            var createResponse = await _fixture.GatewayClient.PostAsJsonAsync("/friendnet/users/create", profileDto);
            createResponse.EnsureSuccessStatusCode();

            var newUserId = await createResponse.Content.ReadAsStringAsync();
            var idStr = newUserId.Trim('"');

            //3. Get the created profile to return it
            var getResponse = await _fixture.GatewayClient.GetAsync($"/friendnet/users/{idStr}");
            getResponse.EnsureSuccessStatusCode();

            var profile = await getResponse.Content.ReadFromJsonAsync<TestingDto.UserProfileOutputDto>();

            return (authDto, profile!, idStr);
        }

        [Fact]
        public async Task RegisterAndGetAllUsersReturnsOkStatusCode()
        {
            // Act
            System.Net.ServicePointManager.SecurityProtocol |= 
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 
                                           | SecurityProtocolType.Tls;

            TestingDto.AuthUserDto newUser = new TestingDto.AuthUserDto
            {
                Email = "dex@gmail.com",
                Password = "dex123",
                Role = "Admin"
            };
            var userJson = System.Text.Json.JsonSerializer.Serialize(newUser);
            var authRequestContent = new StringContent(userJson, Encoding.UTF8, "application/json");
            var authResponse = await _fixture.GatewayClient
                .PostAsync("/friendnet/auth/register", authRequestContent);
            Assert.Equal(HttpStatusCode.OK, authResponse.StatusCode);

            // Extract token from register response and set Authorization header for subsequent requests
            var tokenObj = await authResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (tokenObj != null && tokenObj.TryGetValue("token", out var tokenValue))
            {
                _fixture.GatewayClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
            }

            TestingDto.UserProfileInputDto userProfile1 = new TestingDto.UserProfileInputDto
            {
                UserName = "Dexter",
                Email = "dex@gmail.com"
            };
            var createResponse = _fixture.GatewayClient
                .PostAsJsonAsync("/friendnet/users/create", userProfile1);
            Assert.Equal(HttpStatusCode.OK, createResponse.Result.StatusCode);

            var getAllUsersResponse = await _fixture.GatewayClient
                .GetAsync("/friendnet/users/all", _fixture.CancellationToken);
            // Assert
            List<TestingDto.UserProfileOutputDto>? listUsers = await getAllUsersResponse.Content.ReadFromJsonAsync<List<TestingDto.UserProfileOutputDto>>(_fixture.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, getAllUsersResponse.StatusCode);
            Assert.Equal(1, listUsers!.Count);
        }

        [Fact]
        public async Task Register_And_Login_Success()
        {
            // --- ARRANGE ---
            var uniqueEmail = $"login-test-{Guid.NewGuid()}@friendnet.com";
            var password = "MyPassword123!";
            var authDto = new TestingDto.AuthUserDto { Email = uniqueEmail, Password = password, Role = "Client" };

            //1. Register the user
            var registerResponse = await _fixture.GatewayClient
                .PostAsJsonAsync("/friendnet/auth/register", authDto);

            // --- ACT ---
            //2. Log in with the same user
            var loginResponse = await _fixture.GatewayClient
                .PostAsJsonAsync("/friendnet/auth/login", authDto);
            var token = await loginResponse.Content.ReadAsStringAsync();

            // --- ASSERT ---
            //_outputHelper.WriteLine($"Register Status: {registerResponse.StatusCode}");
            //_outputHelper.WriteLine($"Login Status: {loginResponse.StatusCode}");
            //_outputHelper.WriteLine($"Token: {token}");

            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task Login_Fails_WithWrongPassword()
        {
            // --- ARRANGE ---
            var authDto = new TestingDto.AuthUserDto { Email = $"wrong-pass-{Guid.NewGuid()}@friendnet.com", Password = "RightPassword", Role = "Client" };
            var loginDto = new TestingDto.AuthUserDto { Email = authDto.Email, Password = "WrongPassword", Role = "Client" };

            //1. Register the user
            var registerResponse = await _fixture.GatewayClient.PostAsJsonAsync("/friendnet/auth/register", authDto);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            // --- ACT ---
            //2. Attempt to log in with the wrong password
            var loginResponse = await _fixture.GatewayClient.PostAsJsonAsync("/friendnet/auth/login", loginDto);

            // --- ASSERT ---
            Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);
        }

        [Fact]
        public async Task GetUser_Fails_WithoutAuthentication()
        {
            // --- ARRANGE ---
            // We do *not* log in or register. The client has no cookie.

            // --- ACT ---
            var getResponse = await _fixture.GatewayClient.GetAsync("/friendnet/users/all");

            // --- ASSERT ---
            // This MUST be Unauthorized (401). If it's OK, your auth is broken.
            Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
        }

        [Fact]
        public async Task Edit_User_Success()
        {
            // --- ARRANGE ---
            //1. Create a new user and profile. We are now logged in.
            var (_, createdUser, createdUserId) = await RegisterAndCreateUserAsync("UserToEdit");
            var newUserName = "EditedUserName";
            var editDto = new TestingDto.UserProfileInputDto
            {
                UserName = newUserName,
                Email = createdUser.Email // Email can remain the same
            };

            // --- ACT ---
            //2. Edit the user
            var editResponse = await _fixture.GatewayClient.PatchAsJsonAsync($"/friendnet/users/edit/{createdUserId}", editDto);

            //3. Get the user again to verify the change
            var getResponse = await _fixture.GatewayClient.GetAsync($"/friendnet/users/{createdUserId}");
            var updatedUser = await getResponse.Content.ReadFromJsonAsync<TestingDto.UserProfileOutputDto>();

            // --- ASSERT ---
            _outputHelper.WriteLine($"Edit Status: {editResponse.StatusCode}");
            _outputHelper.WriteLine($"Get Status: {getResponse.StatusCode}");
            _outputHelper.WriteLine($"Original Name: {createdUser.UserName}, New Name: {updatedUser!.UserName}");

            Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.Equal(newUserName, updatedUser!.UserName);
            // ID is not part of the output DTO; ensure the resource id used matches the created id by calling GET by id succeeded
            Assert.False(string.IsNullOrEmpty(createdUserId));
        }

        [Fact]
        public async Task Delete_User_Success()
        {
            // --- ARRANGE ---
            //1. Create a new user and profile. We are now logged in.
            var (_, createdUser, createdUserId) = await RegisterAndCreateUserAsync("UserToDelete");
            _outputHelper.WriteLine($"Created user to delete with ID: {createdUserId}");

            // --- ACT ---
            //2. Delete the user
            var deleteResponse = await _fixture.GatewayClient.DeleteAsync($"/friendnet/users/delete/{createdUserId}");

            //3. Try to get the user again
            var getResponse = await _fixture.GatewayClient.GetAsync($"/friendnet/users/{createdUserId}");

            // --- ASSERT ---
            _outputHelper.WriteLine($"Delete Status: {deleteResponse.StatusCode}");
            _outputHelper.WriteLine($"Get After Delete Status: {getResponse.StatusCode}");

            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            // After deletion the resource must not be found
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task Get_User_By_Email_And_Username_Success()
        {
            // --- ARRANGE ---
            //1. Create a new user and profile. We are now logged in.
            var (authDto, createdUser, createdUserId) = await RegisterAndCreateUserAsync("FindableUser");
            _outputHelper.WriteLine($"Created user {createdUser.UserName} ({createdUser.Email})");

            // --- ACT ---
            //2. Find by Email (Assuming route is "find-by-email")
            var emailResponse = await _fixture.GatewayClient.GetAsync($"/friendnet/users/find-by-email?email={authDto.Email}");
            var userByEmail = await emailResponse.Content.ReadFromJsonAsync<TestingDto.UserProfileOutputDto>();

            //3. Find by Username (Assuming route is "find-by-username")
            var usernameResponse = await _fixture.GatewayClient.GetAsync($"/friendnet/users/find-by-username?userName={createdUser.UserName}");
            // This returns a list, according to your controller signature
            var usersByUsername = await usernameResponse.Content.ReadFromJsonAsync<List<TestingDto.UserProfileOutputDto>>();

            // --- ASSERT ---
            Assert.Equal(HttpStatusCode.OK, emailResponse.StatusCode);
            // Compare by Email since DTO does not contain Id
            Assert.Equal(authDto.Email, userByEmail!.Email);

            Assert.Equal(HttpStatusCode.OK, usernameResponse.StatusCode);
            Assert.NotNull(usersByUsername);
            Assert.Single(usersByUsername);
            Assert.Contains(usersByUsername, u => u.UserName == createdUser.UserName);
        }
    }
}
