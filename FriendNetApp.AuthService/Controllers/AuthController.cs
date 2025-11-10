using AuthService.Data;
using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;
using FriendNetApp.AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private AuthDbContext _context;

    public AuthController(TokenService tokenService, AuthDbContext context)
    {
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("User already exists");
        }
        AppUser newUser = new()
        {
            Email = request.Email,
            Role = request.Role
        };
        var hashedPassword = new PasswordHasher<AppUser>().HashPassword(newUser, request.Password);
        newUser.PasswordHash = hashedPassword;
        var token = _tokenService.GenerateToken(newUser.Id.ToString(), newUser.Email, newUser.Role);
        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // set to true in production (requires HTTPS)
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        });
        return Ok(new { token });
    }

    // Simulate login for demo
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDto request)
    {
        // User user = get user with email = request.email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        var verificationResult = new PasswordHasher<AppUser>().
            VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return BadRequest("Wrong password");
        }
        var token = _tokenService.GenerateToken(user.Id.ToString(), user.Email, user.Role);
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // set to true in production (requires HTTPS)
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        });
        return Ok(token);
    }

    [HttpGet("hello")]
    public async Task<IActionResult> Hello()
    {
        return Ok("Dextar713");
    }
}