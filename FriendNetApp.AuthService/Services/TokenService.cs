using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class TokenService
{
    private const int ExpiryHours = 2;
    private IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId, string email, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        string secretKey = GetSecretKey();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "auth.friendnet",
            audience: "api.friendnet",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(ExpiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GetSecretKey() => _configuration.GetValue<string>("AppSettings:SecretKey") 
                                    ?? Environment.GetEnvironmentVariable("Jwt:SecretKey") 
                                    ?? Environment.GetEnvironmentVariable("JWTSECRET")!;
}