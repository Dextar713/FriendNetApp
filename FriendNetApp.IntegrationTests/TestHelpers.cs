using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FriendNetApp.IntegrationTests
{
    internal static class TestHelpers
    {
        public static async Task<string> RegisterAsync(HttpClient client, string email, string password, string userName)
        {
            var resp = await client.PostAsJsonAsync("/friendnet/auth/register", new
            {
                Email = email,
                Password = password,
                UserName = userName
            });
            resp.EnsureSuccessStatusCode();

            // Try to read structured response
            try
            {
                var dict = await resp.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                if (dict != null && dict.TryGetValue("token", out var t) && !string.IsNullOrEmpty(t))
                    return t!;
            }
            catch
            {
                // ignore
            }

            var raw = await resp.Content.ReadAsStringAsync();
            return raw.Trim('"');
        }

        public static async Task<string> LoginAsync(HttpClient client, string email, string password)
        {
            var resp = await client.PostAsJsonAsync("/friendnet/auth/login", new
            {
                Email = email,
                Password = password,
            });
            resp.EnsureSuccessStatusCode();

            // Try to read structured response
            try
            {
                var dict = await resp.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                if (dict != null && dict.TryGetValue("token", out var t) && !string.IsNullOrEmpty(t))
                    return t!;
            }
            catch
            {
                // ignore
            }

            var raw = await resp.Content.ReadAsStringAsync();
            return raw.Trim('"');
        }

        public static async Task<string> CreateProfileAsync(HttpClient client, string token, TestingDto.UserProfileInputDto profile)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/friendnet/users/create")
            {
                Content = JsonContent.Create(profile)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();
            return body.Trim('"');
        }

        public static async Task CreateFriendshipAsync(HttpClient client, string token, string user1Id, string user2Id)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/friendnet/social/friendships")
            {
                Content = JsonContent.Create(new { User1Id = Guid.Parse(user1Id), User2Id = Guid.Parse(user2Id) })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"CreateFriendship failed: {resp.StatusCode} - {body}");
            }
        }

        public static async Task<MatchDto> FriendMatchAsync(HttpClient client, string token, string userAId, string userBId)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/friendnet/social/matching/from-friend")
            {
                Content = JsonContent.Create(new { UserAId = Guid.Parse(userAId), UserBId = Guid.Parse(userBId) })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"FriendMatch failed: {resp.StatusCode} - {body}");
            }
            return await resp.Content.ReadFromJsonAsync<MatchDto>()!;
        }

        public static async Task<MatchDto> RandomMatchAsync(HttpClient client, string token, string userId)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/friendnet/social/matching/random")
            {
                Content = JsonContent.Create(new { })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<MatchDto>()!;
        }
    }

    public enum MatchType
    {
        FromFriend =0,
        Random =1
    }

    public class MatchDto
    {
        public Guid Id { get; set; }
        public MatchType Type { get; set; }
        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }
        public Guid? InviterId { get; set; }
    }
}