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
        Simulation.KeepConstant(50, TimeSpan.FromSeconds(30))
    );

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();