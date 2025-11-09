using Projects;

var builder = DistributedApplication.CreateBuilder(args);

//DotNetEnv.Env.Load();
//var jwtSecret = Environment.GetEnvironmentVariable("JWTSECRET") ?? DotNetEnv.Env.GetString("JWTSECRET");
//var jwtSecret = builder.AddParameter("JWTSECRET", secret: true);
var jwtSecret = builder.Configuration["JWTSECRET"];

//Console.WriteLine(jwtSecret+" !!!!!!!!!!!!!!!!!!");

var authService = builder
    .AddProject<Projects.FriendNetApp_AuthService>("auth-service")
    .WithEnvironment("Jwt:SecretKey", jwtSecret);

var userProfileService = builder
    .AddProject<Projects.FriendNetApp_UserProfile>("user-profile-service")
    .WithEnvironment("Jwt:SecretKey", jwtSecret);

var gateway = builder
    .AddProject<Projects.FriendNetApp_Gateway>("gateway")
    .WithEnvironment("Jwt:SecretKey", jwtSecret)
    .WithHttpEndpoint(port: 5001, name: "public-http")
    .WithHttpsEndpoint(port: 5000, name: "public-https")
    .WithReference(authService)
    .WithReference(userProfileService);

builder.Build().Run();
