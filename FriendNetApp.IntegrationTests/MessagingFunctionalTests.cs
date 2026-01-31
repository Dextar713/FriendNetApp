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
            //1) Register two users through Auth Service
            var tokenA = await TestHelpers.RegisterAsync(_client, "userA@test.com", "Pa$$w0rd!", "UserA");
            var tokenB = await TestHelpers.RegisterAsync(_client, "userB@test.com", "Pa$$w0rd!", "UserB");

            Assert.NotNull(tokenA);
            Assert.NotNull(tokenB);

            //2) Create user profiles (this triggers UserProfileCreatedEvent)
            var userAId = await TestHelpers.CreateProfileAsync(_client, tokenA, new TestingDto.UserProfileInputDto
            {
                Description = "",
                Age = 21,
                Email = "userA@test.com",
                UserName = "UserA"
            });

            var userBId = await TestHelpers.CreateProfileAsync(_client, tokenB, new TestingDto.UserProfileInputDto
            {
                Description = "",
                Age = 17,
                Email = "userB@test.com",
                UserName = "UserB"
            });

            // Give the event consumers a small moment to store UserReplica
            await Task.Delay(700);

            //3) Create chat through Messaging API
            var createChatResp = await _client.PostAsJsonAsync("/friendnet/messaging/chats/create", new
            {
                User1Id = userAId,
                User2Id = userBId
            });
            var createChatDebug = await createChatResp.Content.ReadAsStringAsync();
            createChatResp.EnsureSuccessStatusCode();

            var chatIdString = (await createChatResp.Content.ReadAsStringAsync()).Trim('"');
            var chatId = Guid.Parse(chatIdString);

            //4) Send message
            var send = await _client.PostAsJsonAsync(
                $"/friendnet/messaging/chats/send",
                new { ChatId = chatId, SenderId = userAId, Content = "hello" });

            Assert.Equal(send.StatusCode, HttpStatusCode.Forbidden);
            send = await _client.PostAsJsonAsync(
                $"/friendnet/messaging/chats/send",
                new { ChatId = chatId, SenderId = userBId, Content = "hello" });

            send.EnsureSuccessStatusCode();
            //5) Get chat history
            var getResp = await _client.GetAsync($"/friendnet/messaging/chats/{chatId}/history");

            getResp.EnsureSuccessStatusCode();

            var messages = await getResp.Content.ReadFromJsonAsync<List<TestingDto.MessageDto>>();
            Assert.NotNull(messages);
            Assert.Single(messages);
            Assert.Equal("hello", messages![0].Content);
            Assert.Equal(userBId, messages[0].SenderId.ToString());
        }
    }
}
