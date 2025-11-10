using Aspire.Hosting;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendNetApp.IntegrationTests
{
    [CollectionDefinition("UserBasic")]
    public class UserBasicCollection : ICollectionFixture<AspireAppFixture>
    {

    }

    public class AspireAppFixture : IAsyncLifetime
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        private IDistributedApplicationTestingBuilder? _appHost;
        private DistributedApplication _app;

        public CancellationToken CancellationToken { get; private set; }
        public HttpClient GatewayClient { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            // Arrange
            CancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
            _appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.FriendNetApp_AppHost>(CancellationToken);
            _appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                // Override the logging filters from the app's configuration
                logging.AddFilter(_appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
            });
            _appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            _app = await _appHost.BuildAsync(CancellationToken).WaitAsync(DefaultTimeout, CancellationToken);
            await _app.StartAsync(CancellationToken).WaitAsync(DefaultTimeout, CancellationToken);

            GatewayClient = _app.CreateHttpClient("gateway");
            await _app.ResourceNotifications.WaitForResourceHealthyAsync("gateway", CancellationToken).WaitAsync(DefaultTimeout, CancellationToken);
        }

        public async Task DisposeAsync()
        {
            await _app.DisposeAsync();
        }
    }
}
