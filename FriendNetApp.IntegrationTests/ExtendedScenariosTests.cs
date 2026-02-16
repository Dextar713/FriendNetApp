using System.Net.Http.Json;
//using Xunit;
using Xunit.Abstractions;

namespace FriendNetApp.IntegrationTests
{
    public class ExtendedScenariosTests : IClassFixture<AspireAppFixture>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public ExtendedScenariosTests(AspireAppFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.GatewayClient;
            _output = output;
        }

        [Fact]
        public async Task FullFlow_WithDeletes_Works()
        {
            // Register users
            var tokenInviter = await TestHelpers.RegisterAsync(_client, "inviter2@test.com", "Pa$$w0rd!", "Admin");
            var inviterId = await TestHelpers.CreateProfileAsync(_client, tokenInviter, new TestingDto.UserProfileInputDto { Email = "inviter2@test.com", UserName = "Inviter2", Age = 30 });
            var tokenA = await TestHelpers.RegisterAsync(_client, "userA2@test.com", "Pa$$w0rd!", "Client");
            var aId = await TestHelpers.CreateProfileAsync(_client, tokenA, new TestingDto.UserProfileInputDto { Email = "userA2@test.com", UserName = "UserA2", Age = 25 });

            var tokenB = await TestHelpers.RegisterAsync(_client, "userB2@test.com", "Pa$$w0rd!", "Client");
            var bId = await TestHelpers.CreateProfileAsync(_client, tokenB, new TestingDto.UserProfileInputDto { Email = "userB2@test.com", UserName = "UserB2", Age = 26 });

            var tokenC = await TestHelpers.RegisterAsync(_client, "userC2@test.com", "Pa$$w0rd!", "Client");
            var cId = await TestHelpers.CreateProfileAsync(_client, tokenC, new TestingDto.UserProfileInputDto { Email = "userC2@test.com", UserName = "UserC2", Age = 27 });

            var tokenD = await TestHelpers.RegisterAsync(_client, "userD2@test.com", "Pa$$w0rd!", "Client");
            var dId = await TestHelpers.CreateProfileAsync(_client, tokenD, new TestingDto.UserProfileInputDto { Email = "userD2@test.com", UserName = "UserD2", Age = 22 });

            // allow consumers to process
            await Task.Delay(9000);
            //_output.WriteLine(aId);
            // Create friendships
            //_output.WriteLine(tokenInviter);
            tokenInviter = await TestHelpers.LoginAsync(_client, "inviter2@test.com", "Pa$$w0rd!");
            //_output.WriteLine(tokenInviter);
            await TestHelpers.CreateFriendshipAsync(_client, tokenInviter, inviterId, aId);
            await TestHelpers.CreateFriendshipAsync(_client, tokenInviter, inviterId, bId);
            await TestHelpers.CreateFriendshipAsync(_client, tokenInviter, inviterId, cId);
            // create A <-> D
            tokenA = await TestHelpers.LoginAsync(_client, "userA2@test.com", "Pa$$w0rd!");

            await TestHelpers.CreateFriendshipAsync(_client, tokenA, aId, dId);

            // wait for eventual consistency
            await Task.Delay(2000);

            // Create chat between A and B as A
            using (var req = new HttpRequestMessage(HttpMethod.Post, "/friendnet/messaging/chats/create") { Content = JsonContent.Create(new { User1Id = aId, User2Id = bId }) })
            {
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
                var resp = await _client.SendAsync(req);
                _output.WriteLine(await resp.Content.ReadAsStringAsync());
                resp.EnsureSuccessStatusCode();
                var chatId = (await resp.Content.ReadAsStringAsync()).Trim('"');

                // Send message as A
                using (var sendReqA = new HttpRequestMessage(HttpMethod.Post, "/friendnet/messaging/chats/send")
                       {
                           Content = JsonContent.Create(new { ChatId = Guid.Parse(chatId), SenderId = Guid.Parse(aId), Content = "hello from A" })
                       })
                {
                    sendReqA.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
                    var sendResp = await _client.SendAsync(sendReqA);
                    sendResp.EnsureSuccessStatusCode();
                }
                //var sendResp = await _client.PostAsJsonAsync("/friendnet/messaging/chats/send", new { ChatId = Guid.Parse(chatId), SenderId = Guid.Parse(aId), Content = "hello from A" });
                //_output.WriteLine(await sendResp.Content.ReadAsStringAsync());
                //sendResp.EnsureSuccessStatusCode();

                // Send message as B (login as B)
                tokenB = await TestHelpers.LoginAsync(_client, "userB2@test.com", "Pa$$w0rd!");
                using var sendReq = new HttpRequestMessage(HttpMethod.Post, "/friendnet/messaging/chats/send") { Content = JsonContent.Create(new { ChatId = Guid.Parse(chatId), SenderId = Guid.Parse(bId), Content = "reply from B" }) };
                sendReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);
                var sendRespB = await _client.SendAsync(sendReq);
                sendRespB.EnsureSuccessStatusCode();

                // Get history
                using var historyReq = new HttpRequestMessage(HttpMethod.Get, $"/friendnet/messaging/chats/{chatId}/history");
                historyReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);
                var historyResp = await _client.SendAsync(historyReq);
                historyResp.EnsureSuccessStatusCode();
                var messages = await historyResp.Content.ReadFromJsonAsync<List<TestingDto.MessageDto>>();
                Assert.NotNull(messages);
                Assert.True(messages!.Count >= 2);
            }

            // Delete friendship inviter <-> B
            using (var delReq = new HttpRequestMessage(HttpMethod.Delete, "/friendnet/social/friendships") { Content = JsonContent.Create(new { User1Id = Guid.Parse(inviterId), User2Id = Guid.Parse(bId) }) })
            {
                tokenInviter = await TestHelpers.LoginAsync(_client, "inviter2@test.com", "Pa$$w0rd!");
                delReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenInviter);
                var delResp = await _client.SendAsync(delReq);
                delResp.EnsureSuccessStatusCode();
            }

            // wait a bit and verify friendship removed (poll)
            bool found = false;
            for (int i = 0; i < 8; i++)
            {
                using var getReq = new HttpRequestMessage(HttpMethod.Get, "/friendnet/social/friendships");
                getReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenInviter);
                var getResp = await _client.SendAsync(getReq);
                getResp.EnsureSuccessStatusCode();
                var list = await getResp.Content.ReadFromJsonAsync<List<CreateFriendshipRequestDto>>();
                if (!PairExists(list!, inviterId, bId))
                {
                    found = true; break;
                }
                await Task.Delay(300);
            }
            Assert.True(found, "Friendship inviter<->B was not removed in time.");

            // Attempt friend match for A and B should now fail
            var ex = await Assert.ThrowsAsync<HttpRequestException>(async () => await TestHelpers.FriendMatchAsync(_client, tokenInviter, aId, bId));
            _output.WriteLine(ex.Message);

            // Delete user B as B
            using (var deleteUserReq = new HttpRequestMessage(HttpMethod.Delete, $"/friendnet/users/delete/{bId}"))
            {
                tokenB = await TestHelpers.LoginAsync(_client, "userB2@test.com", "Pa$$w0rd!");
                deleteUserReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);
                var resp = await _client.SendAsync(deleteUserReq);
                resp.EnsureSuccessStatusCode();
                await Task.Delay(2000); // wait for propagation
            }

            // After deletion, creating a chat with B should fail
            using var createAfterDelReq = new HttpRequestMessage(HttpMethod.Post, "/friendnet/messaging/chats/create") { Content = JsonContent.Create(new { User1Id = aId, User2Id = bId }) };
            tokenA = await TestHelpers.LoginAsync(_client, "userA2@test.com", "Pa$$w0rd!");
            createAfterDelReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
            var createAfterDelResp = await _client.SendAsync(createAfterDelReq);
            Assert.False(createAfterDelResp.IsSuccessStatusCode, "Creating a chat with a deleted user should fail.");
        }

        private static bool PairExists(List<CreateFriendshipRequestDto> list, string id1, string id2)
        {
            foreach (var f in list)
            {
                var s1 = f.User1Id.ToString();
                var s2 = f.User2Id.ToString();
                if ((s1 == id1 && s2 == id2) || (s1 == id2 && s2 == id1))
                    return true;
            }
            return false;
        }
    }
}