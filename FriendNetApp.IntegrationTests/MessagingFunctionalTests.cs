using System.Net.Http.Json;
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
            var tokenA = await TestHelpers.RegisterAsync(_client, "userA@test.com", "Pa$$w0rd!", "Client");
            var tokenB = await TestHelpers.RegisterAsync(_client, "userB@test.com", "Pa$$w0rd!", "Client");

            Assert.NotNull(tokenA);
            Assert.NotNull(tokenB);

            tokenB = await TestHelpers.LoginAsync(_client, "userB@test.com", "Pa$$w0rd!");

            var userBId = await TestHelpers.CreateProfileAsync(_client, tokenB, new TestingDto.UserProfileInputDto
            {
                Description = "",
                Age = 17,
                Email = "userB@test.com",
                UserName = "UserB"
            });

            tokenA = await TestHelpers.LoginAsync(_client, "userA@test.com", "Pa$$w0rd!");
            //2) Create user profiles (this triggers UserProfileCreatedEvent)
            var userAId = await TestHelpers.CreateProfileAsync(_client, tokenA, new TestingDto.UserProfileInputDto
            {
                Description = "",
                Age = 21,
                Email = "userA@test.com",
                UserName = "UserA"
            });

            // Give the event consumers a small moment to store UserReplica
            await Task.Delay(3700);

            //3) Create chat through Messaging API
            var createChatReq = new HttpRequestMessage(HttpMethod.Post, "/friendnet/messaging/chats/create")
            {
                Content = JsonContent.Create(new
                {
                    User1Id = userAId,
                    User2Id = userBId
                })
            };
            createChatReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
            var createChatResp = await _client.SendAsync(createChatReq);
            var createChatDebug = await createChatResp.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(createChatDebug);
            createChatResp.EnsureSuccessStatusCode();

            var chatIdString = (await createChatResp.Content.ReadAsStringAsync()).Trim('"');
            var chatId = Guid.Parse(chatIdString);

            //4) Send message
            var sendReq = new HttpRequestMessage(HttpMethod.Post, "/friendnet/messaging/chats/send")
            {
                Content = JsonContent.Create(new
                {
                    ChatId = chatId,
                    SenderId = userAId,
                    Content = "hello"
                })
            };
            sendReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
            var send = await _client.SendAsync(sendReq);
            Assert.Equal(HttpStatusCode.OK, send.StatusCode);

            send.EnsureSuccessStatusCode();
            //5) Get chat history
            var getHistoryReq = new HttpRequestMessage(HttpMethod.Get, $"/friendnet/messaging/chats/{chatId}/history");
            getHistoryReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
            var getResp = await _client.SendAsync(getHistoryReq);

            getResp.EnsureSuccessStatusCode();

            var messages = await getResp.Content.ReadFromJsonAsync<List<TestingDto.MessageDto>>();
            Assert.NotNull(messages);
            Assert.Single(messages);
            Assert.Equal("hello", messages![0].Content);
            Assert.Equal(userAId, messages[0].SenderId.ToString());
        }
    }
}
