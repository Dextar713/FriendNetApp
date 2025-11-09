using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddServiceDiscovery();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

// Get JWT secret from configuration or environment variable
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
app.UseAuthentication();

app.MapReverseProxy();

app.Run();
