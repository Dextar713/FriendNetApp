using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace FriendNetApp.IntegrationTests
{
    public class MessagingSignalRTests : IClassFixture<AspireAppFixture>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _testOutputHelper;


        public MessagingSignalRTests(AspireAppFixture fixture,
            ITestOutputHelper testOutputHelper)
        {
            _client = fixture.GatewayClient;   
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task SignalR_Broadcasts_Message_To_Other_User()
        {
            //1) Register and create profiles for two users
            var tokenA = await TestHelpers.RegisterAsync(_client, "signalrA@test.com", "Pa$$w0rd!", "Client");
            var tokenB = await TestHelpers.RegisterAsync(_client, "signalrB@test.com", "Pa$$w0rd!", "Client");

            tokenA = await TestHelpers.LoginAsync(_client, "signalrA@test.com", "Pa$$w0rd!");
            var userAId = await TestHelpers.CreateProfileAsync(_client, tokenA, new TestingDto.UserProfileInputDto { Email = "signalrA@test.com", UserName = "SignalA", Age = 21 });

            tokenB = await TestHelpers.LoginAsync(_client, "signalrB@test.com", "Pa$$w0rd!");

            var userBId = await TestHelpers.CreateProfileAsync(_client, tokenB, new TestingDto.UserProfileInputDto { Email = "signalrB@test.com", UserName = "SignalB", Age =22 });

            // allow consumers to process replicas
            await Task.Delay(2000);

            //2) Create chat between A and B
            using var createReq = new HttpRequestMessage(HttpMethod.Post, "/friendnet/messaging/chats/create")
            {
                Content = JsonContent.Create(new { User1Id = userAId, User2Id = userBId })
            };
            createReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
            var createResp = await _client.SendAsync(createReq);
            var respContent = await createResp.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(respContent + " !!!!!\n-----------\n");
            createResp.EnsureSuccessStatusCode();
            var chatIdString = (await createResp.Content.ReadAsStringAsync()).Trim('"');

            //3) Setup Hub connections for A and B to the gateway hub endpoint
            var baseUri = _client.BaseAddress ?? new Uri("http://localhost:5001");
            var hubRelative = $"/friendnet/messaging/hubs/chat?chatId={chatIdString}";
            var hubUri = new Uri(baseUri, hubRelative).ToString();

            // TaskCompletionSource to capture message on B
            var tcs = new TaskCompletionSource<TestingDto.MessageDto>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Configure connection options to accept test server certs and provide access token
            HubConnection CreateConnection(string token)
            {
                return new HubConnectionBuilder()
                    .WithUrl(hubUri, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                        options.Transports = HttpTransportType.WebSockets;
                        options.HttpMessageHandlerFactory = _ =>
                        {
                            var handler = new HttpClientHandler();
                            handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                            return handler;
                        };
                    })
                    .WithAutomaticReconnect()
                    .Build();
            }

            var connB = CreateConnection(tokenB);
            connB.On<TestingDto.MessageDto>("ReceiveMessage", (msg) =>
            {
                _testOutputHelper.WriteLine($"ConnB received: {msg.Content}");
                tcs.TrySetResult(msg);
            });

            var connA = CreateConnection(tokenA);

            await connB.StartAsync();
            await connA.StartAsync();

            try
            {
                //4) Send message from A via hub
                var message = new TestingDto.MessageDto
                {
                    ChatId = Guid.Parse(chatIdString),
                    SenderId = Guid.Parse(userAId),
                    Content = "hello from A"
                };

                // Invoke the hub method 'SendMessage' which expects a Command object { Message = MessageDto }
                await connA.InvokeAsync("SendMessage", new { Message = message });

                //5) Assert B receives it
                var received = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.NotNull(received);
                Assert.Equal(message.Content, received.Content);
                Assert.Equal(message.SenderId, received.SenderId);
                Assert.Equal(message.ChatId, received.ChatId);
            }
            finally
            {
                await connA.DisposeAsync();
                await connB.DisposeAsync();
            }
        }
    }
}
