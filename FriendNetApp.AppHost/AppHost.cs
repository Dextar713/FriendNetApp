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

var rabbit = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var userProfileService = builder
    .AddProject<Projects.FriendNetApp_UserProfile>("user-profile-service")
    .WithReference(rabbit)
    .WithEnvironment("Jwt:SecretKey", jwtSecret);

var messagingService = builder
    .AddProject<FriendNetApp_MessagingService>("messaging-service")
    .WithReference(rabbit)
    .WithEnvironment("Jwt:SecretKey", jwtSecret);

var socialService = builder
    .AddProject<Projects.FriendNetApp_SocialService>("social-service")
    .WithReference(rabbit)
    .WithEnvironment("Jwt:SecretKey", jwtSecret);

var gateway = builder
    .AddProject<Projects.FriendNetApp_Gateway>("gateway")
    .WithEnvironment("Jwt:SecretKey", jwtSecret)
    .WithHttpEndpoint(port: 5001, name: "public-http")
    .WithHttpsEndpoint(port: 5000, name: "public-https")
    .WithReference(authService)
    .WithReference(userProfileService)
    .WithReference(messagingService)
    .WithReference(socialService);

builder.Build().Run();
