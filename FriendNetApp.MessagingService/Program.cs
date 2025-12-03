using FriendNetApp.MessagingService.Data;
using FriendNetApp.MessagingService.Dto;
using FriendNetApp.MessagingService.Hubs;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using FriendNetApp.MessagingService.Dto.Events;
using FriendNetApp.MessagingService.Services;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();


builder.Services.AddDbContext<MessagingDbContext>(options =>
{
    options.UseInMemoryDatabase("MessagingDb");
});


builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile(new AutoMapperProfile());
});

/*
var rabbitHost = config["RabbitMq:Host"] ?? Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var rabbitUser = config["RabbitMq:User"] ?? Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitPass = config["RabbitMq:Password"] ?? Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";
var rabbitVHost = config["RabbitMq:VirtualHost"] ?? Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/";
var rabbitPort = config["RabbitMq:Port"] ?? Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672";
*/

var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq");

builder.Services.AddMassTransit(cfg =>
{ 
    cfg.AddConsumer<UserCreatedEventConsumer>();
    cfg.AddConsumer<UserUpdatedEventConsumer>();

    cfg.UsingRabbitMq((context, busCfg) =>
    {
        busCfg.Host(rabbitMqConnectionString);
        busCfg.ConfigureEndpoints(context);

        /*
        busCfg.ReceiveEndpoint("messaging-service-user-events", e =>
        {
            e.ConfigureConsumer<UserCreatedEventConsumer>(context);
            e.ConfigureConsumer<UserUpdatedEventConsumer>(context);
        });
        */
    });
});



var assembly = Assembly.GetExecutingAssembly();

var nestedHandlers = assembly.GetTypes()
    .Where(t => t.IsNested && t.Name == "Handler" && !t.IsAbstract && !t.IsInterface);

foreach (var handlerType in nestedHandlers)
{
    // Register the nested Handler class as itself or as interfaces if implemented
    builder.Services.AddScoped(handlerType);
}

var jwtSecret = config["Jwt:SecretKey"] ?? Environment.GetEnvironmentVariable("Jwt:SecretKey") ?? Environment.GetEnvironmentVariable("JWTSECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT secret key is not configured");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Read the token from the "jwt" cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/hubs/chat");
app.MapControllers();

app.Run();
