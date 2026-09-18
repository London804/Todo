using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.Dtos;

// What a client sends to begin a password reset.
public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
