using Authentication_API;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } // "Student" or "Teacher"
}

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {
        var user = new User { Email = dto.Email, Role = dto.Role };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok("Registered");
    }

    /* Runs when the MVC client sends a request to this endpoint 
     * This endpoint checks if the password sent is correct and creates a cookie on line 70
     *  */
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
        { 
            return Unauthorized();
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
        { 
            return Unauthorized(); 
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        /* BTS, this makes the API server attach a "Set-Cookie" header to its HTTP response
         * Who receives this? Not the browser, the MVC server's HttpClient receives it
         because that was who made the request initially in the MVC. 
         * The actual humans browser did not know this exchange even happened */
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Ok("Logged in");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok("Logged out");
    }
}