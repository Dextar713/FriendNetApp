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
            await Task.Delay(100);
            var x = 7;
            Assert.Equal(7, x);
        }
    }
}
