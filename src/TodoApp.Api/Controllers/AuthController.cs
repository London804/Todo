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
    // into a signed JWT. IEmailSender delivers the reset link; IConfiguration gives
    // us the frontend base URL to build that link.
    private readonly UserManager<ApplicationUser> _users;
    private readonly TokenService _tokens;
    private readonly IEmailSender _email;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> users,
        TokenService tokens,
        IEmailSender email,
        IConfiguration config,
        ILogger<AuthController> logger)
    {
        _users = users;
        _tokens = tokens;
        _email = email;
        _config = config;
        _logger = logger;
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
            {
                // We use the email as the username, so a duplicate username is the
                // same fact as a duplicate email — skip the redundant message.
                if (error.Code == nameof(IdentityErrorDescriber.DuplicateUserName))
                    continue;
                ModelState.AddModelError(error.Code, error.Description);
            }
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

    // POST /api/auth/forgot-password  { "email": "..." }
    // Starts a password reset: if the email belongs to a real account, generate a
    // reset token and email a link containing it.
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            // A signed, time-limited token (default lifespan: 1 day). It must be
            // URL-encoded because it can contain characters unsafe in a URL.
            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var baseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var link = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(request.Email)}" +
                       $"&token={Uri.EscapeDataString(token)}";

            try
            {
                await _email.SendAsync(
                    request.Email,
                    "Reset your TodoApp password",
                    $"Click the link to reset your password:\n{link}");
            }
            catch (Exception ex)
            {
                // Don't let an email failure change the response — that would both
                // break the UX and leak which emails are registered. Log and move on.
                _logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
            }
        }

        // ALWAYS return the same response, whether or not the email exists — this
        // prevents attackers from using this endpoint to discover valid accounts.
        return Ok(new { message = "If that email is registered, a reset link has been sent." });
    }

    // POST /api/auth/reset-password  { "email": "...", "token": "...", "newPassword": "..." }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);

        // Generic message for a missing user OR a bad/expired token — don't reveal
        // which. (A valid token can only have come from the email anyway.)
        if (user is null)
            return BadRequest(new ProblemDetails { Title = "Invalid or expired reset request." });

        var result = await _users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                // An invalid/expired token surfaces here; keep it generic.
                if (error.Code == nameof(IdentityErrorDescriber.InvalidToken))
                    return BadRequest(new ProblemDetails { Title = "Invalid or expired reset request." });
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        return Ok(new { message = "Password has been reset. You can now log in." });
    }
}
