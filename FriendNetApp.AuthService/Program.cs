using AuthService.Data;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ensure the service listens on a fixed port so AppHost can expose it
//builder.WebHost.UseUrls("http://localhost:7777");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    //options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb"));
    options.UseInMemoryDatabase("AuthDb");
});

builder.Services.AddSingleton<TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<AuthDbContext>();
    await context.Database.MigrateAsync();
    //await DbInitializer.SeedData(context);
}
catch (Exception ex)
{
    // Log the error (uncomment ex variable name and write a log.)
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database:");
}

app.Run();
