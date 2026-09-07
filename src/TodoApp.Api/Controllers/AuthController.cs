using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Auth;
using TodoApp.Api.Dtos;
using TodoApp.Api.Models;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // -> /api/auth
public class AuthController : ControllerBase
{
    // UserManager is Identity's service for creating/finding users and checking
    // passwords — it handles password hashing for us. TokenService turns a user
    // into a signed JWT.
    private readonly UserManager<ApplicationUser> _users;
    private readonly TokenService _tokens;

    public AuthController(UserManager<ApplicationUser> users, TokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    // POST /api/auth/register  { "email": "...", "password": "..." }
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email, // use the email as the username
            Email = request.Email,
        };

        // CreateAsync hashes the password and stores the user. It also enforces
        // Identity's password rules (length, complexity, etc.).
        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // Surface Identity's validation messages (e.g. "email already taken").
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return ValidationProblem(ModelState); // 400 with details
        }

        return Ok(new AuthResponse { Token = _tokens.CreateToken(user), Email = user.Email! });
    }

    // POST /api/auth/login  { "email": "...", "password": "..." }
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);

        // Deliberately vague: don't reveal whether it was the email or the
        // password that was wrong — that helps prevent account enumeration.
        if (user is null || !await _users.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new ProblemDetails { Title = "Invalid email or password." });

        return Ok(new AuthResponse { Token = _tokens.CreateToken(user), Email = user.Email! });
    }
}
