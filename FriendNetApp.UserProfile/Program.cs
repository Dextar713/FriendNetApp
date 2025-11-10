using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserProfileDbContext>(options =>
{
    options.UseInMemoryDatabase("UserProfileDb");
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile(new AutoMapperProfile());
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

// Debug logging
//Console.WriteLine($"Env Jwt:SecretKey: {Environment.GetEnvironmentVariable("Jwt:SecretKey")}");
//Console.WriteLine($"Env JWTSECRET: {Environment.GetEnvironmentVariable("JWTSECRET")}");
//Console.WriteLine($"Final JWT Secret (length): {jwtSecret?.Length ?? 0}");

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

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<UserProfileDbContext>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(context);
}
catch (Exception ex)
{
    // Log the error (uncomment ex variable name and write a log.)
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database:");
}


app.Run();
