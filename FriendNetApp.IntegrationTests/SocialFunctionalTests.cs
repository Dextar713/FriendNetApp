using System.Net.Http.Json;
//using Xunit;
using Xunit.Abstractions;

namespace FriendNetApp.IntegrationTests
{
    public class SocialFunctionalTests : IClassFixture<AspireAppFixture>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _testOutputHelper;

        public SocialFunctionalTests(AspireAppFixture fixture,
        ITestOutputHelper testOutputHelper)
        {
            _client = fixture.GatewayClient;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Friendships_And_FriendMatch_Works()
        {
            // Register three users: inviter, friendA, friendB
            var tokenInviter = await TestHelpers.RegisterAsync(_client, "inviter@test.com", "Pa$$w0rd!", "Admin");
            var inviterId = await TestHelpers.CreateProfileAsync(_client, tokenInviter, new TestingDto.UserProfileInputDto { Email = "inviter@test.com", UserName = "Inviter", Age = 30 });

            var tokenA = await TestHelpers.RegisterAsync(_client, "friendA@test.com", "Pa$$w0rd!", "Client");
            var aId = await TestHelpers.CreateProfileAsync(_client, tokenA, new TestingDto.UserProfileInputDto { Email = "friendA@test.com", UserName = "FriendA", Age = 25 });

            var tokenB = await TestHelpers.RegisterAsync(_client, "friendB@test.com", "Pa$$w0rd!", "Client");
            var bId = await TestHelpers.CreateProfileAsync(_client, tokenB, new TestingDto.UserProfileInputDto { Email = "friendB@test.com", UserName = "FriendB", Age =26 });

            // Wait for replicas / consumers
            await Task.Delay(3000);
            //_testOutputHelper.WriteLine(inviterId);

            // Create friendships: inviter <-> A and inviter <-> B
            tokenInviter = await TestHelpers.LoginAsync(_client, "inviter@test.com", "Pa$$w0rd!");
            await TestHelpers.CreateFriendshipAsync(_client, tokenInviter, inviterId, aId);
            await TestHelpers.CreateFriendshipAsync(_client, tokenInviter, inviterId, bId);

            // Verify friendships via GET
            using (var req = new HttpRequestMessage(HttpMethod.Get, "/friendnet/social/friendships"))
            {
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenInviter);
                var resp = await _client.SendAsync(req);
                resp.EnsureSuccessStatusCode();
                var list = await resp.Content.ReadFromJsonAsync<List<CreateFriendshipRequestDto>>();
                _testOutputHelper.WriteLine($"Friendships for inviter: {list?.Count}");

                bool pairInviterA = PairExists(list!, inviterId, aId);
                bool pairInviterB = PairExists(list!, inviterId, bId);

                if (!pairInviterA || !pairInviterB)
                {
                    _testOutputHelper.WriteLine("Friendships content:");
                    foreach (var f in list!)
                    {
                        _testOutputHelper.WriteLine($" - {f.User1Id} / {f.User2Id}");
                    }
                }

                Assert.True(pairInviterA, "Inviter<->A friendship was not found in the returned list.");
                Assert.True(pairInviterB, "Inviter<->B friendship was not found in the returned list.");
            }

            // Try friend match (inviter creates a match for A and B)
            var match = await TestHelpers.FriendMatchAsync(_client, tokenInviter, aId, bId);
            Assert.NotNull(match);
            Assert.Equal(MatchType.FromFriend, match.Type);
            Assert.Equal(inviterId, match.InviterId?.ToString());
        }

        [Fact]
        public async Task RandomMatch_Works()
        {
            var token = await TestHelpers.RegisterAsync(_client, "rand@test.com", "Pa$$w0rd!", "Client");
            var userId = await TestHelpers.CreateProfileAsync(_client, token, new TestingDto.UserProfileInputDto { Email = "rand@test.com", UserName = "RandUser", Age =28 });
            await Task.Delay(800);
            var match = await TestHelpers.RandomMatchAsync(_client, token, userId);
            Assert.NotNull(match);
            Assert.Equal(MatchType.Random, match.Type);
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

    // Local DTO for assertions
    public class CreateFriendshipRequestDto
    {
        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }
    }
    
}