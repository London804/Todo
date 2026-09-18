using System.ComponentModel.DataAnnotations;
using TodoApp.Api.Validation;

namespace TodoApp.Api.Dtos;

// What a client sends to complete a password reset: the email, the reset token
// they received (via the emailed link), and their chosen new password.
public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [NotWhitespace(ErrorMessage = "Password cannot be empty or whitespace.")]
    public string NewPassword { get; set; } = string.Empty;
}
