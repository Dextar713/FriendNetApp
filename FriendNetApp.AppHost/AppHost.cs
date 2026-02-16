using Projects;

var builder = DistributedApplication.CreateBuilder(args);

//DotNetEnv.Env.Load();
//var jwtSecret = Environment.GetEnvironmentVariable("JWTSECRET") ?? DotNetEnv.Env.GetString("JWTSECRET");
//var jwtSecret = builder.AddParameter("JWTSECRET", secret: true);
var jwtSecret = builder.Configuration["JWTSECRET"];

var postgresPassword = builder.AddParameter("postgres-password", "postgres");
var postgresUsername = builder.AddParameter("postgres-username", "postgres");
var postgres = builder.AddPostgres("postgres")
    .WithUserName(postgresUsername)
    .WithPassword(postgresPassword)
    .WithImagePullPolicy(ImagePullPolicy.Missing);

var authDb = postgres.AddDatabase("authdb");
var userProfileDb = postgres.AddDatabase("user-profile-db");
var messagingDb = postgres.AddDatabase("messaging-db");
var socialDb = postgres.AddDatabase("social-db");

var authService = builder
    .AddProject<Projects.FriendNetApp_AuthService>("auth-service")
    .WithEnvironment("Jwt:SecretKey", jwtSecret)
    .WithReference(authDb)
    .WaitFor(postgres);
    //.WithReplicas(3);

var rabbit = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithImagePullPolicy(ImagePullPolicy.Missing);

var userProfileService = builder
    .AddProject<Projects.FriendNetApp_UserProfile>("user-profile-service")
    .WithReference(rabbit)
    .WithEnvironment("Jwt:SecretKey", jwtSecret)
    .WithReference(userProfileDb)
    .WaitFor(postgres);

var messagingService = builder
    .AddProject<FriendNetApp_MessagingService>("messaging-service")
    .WithReference(rabbit)
    .WithEnvironment("Jwt:SecretKey", jwtSecret)
    .WithReference(messagingDb)
    .WaitFor(postgres);

var socialService = builder
    .AddProject<Projects.FriendNetApp_SocialService>("social-service")
    .WithReference(rabbit)
    .WithEnvironment("Jwt:SecretKey", jwtSecret)
    .WithReference(socialDb)
    .WaitFor(postgres);

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
