using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber.CSharp;

var baseUrl = "http://localhost:5001";

var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};
try
{
    var registerResponse = await httpClient.PostAsJsonAsync("/friendnet/auth/register",
        new { Email = "user@test.com", Password = "Pa$$w0rd!" });
    if (!registerResponse.IsSuccessStatusCode)
    {
        throw new Exception("Register failed");
    }
} catch (Exception ex)
{
    Console.WriteLine($"Error during registration: {ex.Message}");
}

var scenario = Scenario.Create("auth_load", async context =>
    {
        return await Step.Run("login_request", context, async () =>
        {
            var response = await httpClient.PostAsJsonAsync(
                "/friendnet/auth/login",
                new { Email = "user@test.com", Password = "Pa$$w0rd!" });

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        });
    })
    .WithLoadSimulations(
        Simulation.KeepConstant(10, TimeSpan.FromSeconds(30))
    );

var profile_scenario = Scenario.Create("create_profile", async context =>
    {
        var randomId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"load_{randomId}@test.com";
        var password = "DexPass123!";
        var userName = $"User_{randomId}";
        string token = "";
        var registerStep = await Step.Run("register", context, async () =>
        {
            var response = await httpClient.PostAsJsonAsync(
                "/friendnet/auth/register",
                new { Email = email, Password = password, Role="Client" });

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(statusCode:response.StatusCode.ToString(), message:"Register failed");
        });
        if (registerStep.IsError)
        {
            return Response.Fail();
        }

        var loginStep = await Step.Run("login", context, async () =>
        {
            var response = await httpClient.PostAsJsonAsync("/friendnet/auth/login",
                new
                {
                    Email = email,
                    Password = password
                });
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return Response.Fail(statusCode:response.StatusCode.ToString(), message:"Login failed" + errorMessage);
            }

            token = await response.Content.ReadAsStringAsync();
            return !string.IsNullOrEmpty(token) ? Response.Ok() : Response.Fail(message:"No token");

        });
        if (loginStep.IsError)
        {
            return Response.Fail();
        }

        var createProfileStep = await Step.Run("create_profile", context, async () =>
        {
            var profileDto = new
            {
                UserName = userName,
                Email = email,
                Age = 25
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/friendnet/users/create");
            request.Content = JsonContent.Create(profileDto);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Response.Fail(statusCode: response.StatusCode.ToString(), message: "Create profile failed");
            }

            return Response.Ok();

        });
        return createProfileStep;
    })
    .WithLoadSimulations(
        Simulation.KeepConstant(copies: 10, TimeSpan.FromSeconds(20))
    );

NBomberRunner
    .RegisterScenarios(profile_scenario)
    .Run();