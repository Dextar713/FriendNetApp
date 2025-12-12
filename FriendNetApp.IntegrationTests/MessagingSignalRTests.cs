using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FriendNetApp.IntegrationTests
{
    public class MessagingSignalRTests : IClassFixture<AspireAppFixture>
    {
        private readonly AspireAppFixture _fixture;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _testOutputHelper;


        public MessagingSignalRTests(AspireAppFixture fixture,
            ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _client = fixture.GatewayClient;   
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task SignalR_Broadcasts_Message_To_Other_User()
        {
            var regA = await _client.PostAsJsonAsync("/friendnet/auth/register", new
            {
                Email = "userA@test.com",
                Password = "Pa$$w0rd!",
                UserName = "UserA"
            });

            regA.EnsureSuccessStatusCode();

            //var authA = await regA.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            //var tokenA = authA["token"];
            //var userAId = Guid.Parse(authA["userId"]);   // must be returned by your auth service

            var tokenA = await regA.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(tokenA);
            Assert.NotNull(tokenA);

            var regB = await _client.PostAsJsonAsync("/friendnet/auth/register", new
            {
                Email = "userB@test.com",
                Password = "Pa$$w0rd!",
                UserName = "UserB"
            });

            regB.EnsureSuccessStatusCode();

            //var authB = await regB.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            //var tokenB = authB["token"];
            //var userBId = Guid.Parse(authB["userId"]);

            var tokenB = await regB.Content.ReadAsStringAsync();
            Assert.NotNull(tokenB);
            Assert.Null(tokenA);

            /*
            await _client.PostAsJsonAsync("/profile/create", new
            {
                UserId = userAId,
                DisplayName = "UserA"
            });

            await _client.PostAsJsonAsync("/profile/create", new
            {
                UserId = userBId,
                DisplayName = "UserB"
            });

            // Wait a moment for RabbitMQ event → MessagingService → consume
            await Task.Delay(1500);

            // ---------------------------
            // 4. Create chat through MessagingService
            // ---------------------------
            var chatResp = await _client.PostAsJsonAsync("/messaging/chats/create", new
            {
                User1Id = userAId,
                User2Id = userBId
            });

            chatResp.EnsureSuccessStatusCode();
            var chatId = (await chatResp.Content.ReadAsStringAsync()).Trim('"');

            // ---------------------------
            // 5. Connect SignalR for both users
            // ---------------------------
            var hubUrl = new Uri(_client.BaseAddress!, "/hubs/chat").ToString();

            var connectionA = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(tokenA);
                })
                .WithAutomaticReconnect()
                .Build();

            var connectionB = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(tokenB);
                })
                .WithAutomaticReconnect()
                .Build();


            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            connectionB.On<TestingDto.MessageDto>("ReceiveMessage", msg =>
            {
                tcs.TrySetResult(msg.Content);
            });

            await connectionA.StartAsync();
            await connectionB.StartAsync();

            // ---------------------------
            // 6. Join both to same chat group
            // ---------------------------
            await connectionA.InvokeAsync("JoinChatGroup", chatId);
            await connectionB.InvokeAsync("JoinChatGroup", chatId);

            // ---------------------------
            // 7. Send message from User A
            // ---------------------------
            await connectionA.InvokeAsync("SendMessageToChat", chatId, "hello world");

            // ---------------------------
            // 8. Assert User B receives it
            // ---------------------------
            var received = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal("hello world", received);

            // cleanup
            await connectionA.StopAsync();
            await connectionB.StopAsync();
            */
        }
    }
}
