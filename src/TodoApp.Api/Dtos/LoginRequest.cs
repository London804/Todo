using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.Dtos;

// What a client sends to log in to an existing account.
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
