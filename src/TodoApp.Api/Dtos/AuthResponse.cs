namespace TodoApp.Api.Dtos;

// What the API returns after a successful register or login: the JWT the client
// then sends on every subsequent request (in the "Authorization: Bearer ..." header).
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
