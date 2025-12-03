using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace FriendNetApp.IntegrationTests
{
    public class MessagingFunctionalTests : IClassFixture<AspireAppFixture>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _testOutputHelper;

        public MessagingFunctionalTests(AspireAppFixture fixture,
            ITestOutputHelper testOutputHelper)
        {
            _client = fixture.GatewayClient;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task MessagingFlow_EndToEnd_Works()
        {
            // 1) Register two users through Auth Service
            var regA = await _client.PostAsJsonAsync("/friendnet/auth/register", new
            {
                Email = "userA@test.com",
                Password = "Pa$$w0rd!",
                UserName = "UserA"
            });

            regA.EnsureSuccessStatusCode();

            var regB = await _client.PostAsJsonAsync("/friendnet/auth/register", new
            {
                Email = "userB@test.com",
                Password = "Pa$$w0rd!",
                UserName = "UserB"
            });

            regB.EnsureSuccessStatusCode();

            // extract Ids returned from Auth service (JSON { id, email, role })
             //_testOutputHelper.WriteLine(await regA.Content.ReadAsStringAsync());
            var tokenA = await regA.Content.ReadAsStringAsync();
            var tokenB = await regB.Content.ReadAsStringAsync();

            Assert.NotNull(tokenA);
            Assert.NotNull(tokenB);
            

            // 2) Create user profiles (this triggers UserProfileCreatedEvent)
            var profA = await _client.PostAsJsonAsync("/friendnet/users/create", new
            {
                Bio = "",
                Avatar = "",
                Email = "userA@test.com",
                UserName = "UserA"
            });
            var outputA = await profA.Content.ReadAsStringAsync();
            //_testOutputHelper.WriteLine(outputA);
            profA.EnsureSuccessStatusCode();

            var profB = await _client.PostAsJsonAsync("/friendnet/users/create", new
            {
                Bio = "",
                Avatar = "",
                Email = "userB@test.com",
                UserName = "UserB"
            });

            profB.EnsureSuccessStatusCode();
            var userAId = (await profA.Content.ReadAsStringAsync()).Trim('"');
            var userBId = (await profB.Content.ReadAsStringAsync()).Trim('"');
            // Give the event consumers a small moment to store UserReplica
            await Task.Delay(700);

            // 3) Create chat through Messaging API
            var createChatResp = await _client.PostAsJsonAsync("/friendnet/messaging/chats/create", new
            {
                User1Id = userAId,
                User2Id = userBId
            });
            var createChatDebug = await createChatResp.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(createChatDebug);
            _testOutputHelper.WriteLine("UserAId: "+userAId);
            _testOutputHelper.WriteLine("UserBId: "+userBId);
            _testOutputHelper.WriteLine("A len: "+userAId.Length);
            createChatResp.EnsureSuccessStatusCode();

            var chatIdString = (await createChatResp.Content.ReadAsStringAsync()).Trim('"');
            var chatId = Guid.Parse(chatIdString);

            // 4) Send message
            var send = await _client.PostAsJsonAsync(
                $"/friendnet/messaging/chats/send",
                new { ChatId = chatId, SenderId = userAId, Content = "hello" });

            Assert.Equal(send.StatusCode, HttpStatusCode.Forbidden);
            send = await _client.PostAsJsonAsync(
                $"/friendnet/messaging/chats/send",
                new { ChatId = chatId, SenderId = userBId, Content = "hello" });

            send.EnsureSuccessStatusCode();
            // 5) Get chat history
            var getResp = await _client.GetAsync($"/friendnet/messaging/chats/{chatId}/history");

            getResp.EnsureSuccessStatusCode();

            var messages = await getResp.Content.ReadFromJsonAsync<List<TestingDto.MessageDto>>();

            Assert.Single(messages);
            Assert.Equal("hello", messages![0].Content);
            Assert.Equal(userBId, messages[0].SenderId.ToString());
        }
    }
}
