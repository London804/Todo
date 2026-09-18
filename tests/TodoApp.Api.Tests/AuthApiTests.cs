using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Api.Dtos;

namespace TodoApp.Api.Tests;

// Tests for authentication (register/login) and the per-user isolation it enables.
public class AuthApiTests : IDisposable
{
    private readonly TodoAppFactory _factory;

    public AuthApiTests()
    {
        _factory = new TodoAppFactory();
    }

    public void Dispose() => _factory.Dispose();

    private async Task<string> RegisterAsync(HttpClient client, string email, string password = "Password123!")
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    private static void Authenticate(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    [Fact]
    public async Task Register_ReturnsToken()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "new@test.com", password = "Password123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.Equal("new@test.com", auth.Email);
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "short@test.com", password = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        await RegisterAsync(client, "dupe@test.com");

        var second = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "dupe@test.com", password = "Password123!" });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var client = _factory.CreateClient();
        await RegisterAsync(client, "login@test.com");

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "login@test.com", password = "Password123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        await RegisterAsync(client, "wrongpw@test.com");

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "wrongpw@test.com", password = "WrongPassword1!" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Todos_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient(); // no Authorization header
        var response = await client.GetAsync("/api/todos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Todos_AreIsolatedPerUser()
    {
        // User A creates a todo.
        var clientA = _factory.CreateClient();
        Authenticate(clientA, await RegisterAsync(clientA, "usera@test.com"));
        var created = await clientA.PostAsJsonAsync("/api/todos", new { title = "A's private todo" });
        created.EnsureSuccessStatusCode();
        var todoA = (await created.Content.ReadFromJsonAsync<TodoResponse>())!;

        // User B logs in separately.
        var clientB = _factory.CreateClient();
        Authenticate(clientB, await RegisterAsync(clientB, "userb@test.com"));

        // B's list must not contain A's todo...
        var bList = await clientB.GetFromJsonAsync<List<TodoResponse>>("/api/todos");
        Assert.DoesNotContain(bList!, t => t.Id == todoA.Id);

        // ...and B cannot fetch A's todo directly (404, not 403 — we don't reveal existence).
        var bGet = await clientB.GetAsync($"/api/todos/{todoA.Id}");
        Assert.Equal(HttpStatusCode.NotFound, bGet.StatusCode);
    }

    // --- password reset ---

    // Pulls the reset token out of the link in the captured email body.
    private string GetResetTokenFromEmail()
    {
        var sender = _factory.Services.GetRequiredService<CapturingEmailSender>();
        var body = sender.Sent.Single().Body;
        const string marker = "token=";
        return Uri.UnescapeDataString(body[(body.IndexOf(marker) + marker.Length)..].Trim());
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsOkAndSendsNothing()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email = "nobody@test.com" });

        // Always 200 (no account enumeration), and no email is actually sent.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sender = _factory.Services.GetRequiredService<CapturingEmailSender>();
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task ForgotPassword_KnownEmail_SendsResetEmail()
    {
        var client = _factory.CreateClient();
        await RegisterAsync(client, "forgot@test.com");

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email = "forgot@test.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sender = _factory.Services.GetRequiredService<CapturingEmailSender>();
        var email = Assert.Single(sender.Sent);
        Assert.Equal("forgot@test.com", email.To);
        Assert.Contains("token=", email.Body);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ChangesPassword()
    {
        var client = _factory.CreateClient();
        await RegisterAsync(client, "reset@test.com", "OldPassword123!");

        // Kick off the reset and grab the token from the "email".
        await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "reset@test.com" });
        var token = GetResetTokenFromEmail();

        var reset = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { email = "reset@test.com", token, newPassword = "NewPassword123!" });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        // Old password no longer works; new one does.
        var oldLogin = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "reset@test.com", password = "OldPassword123!" });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "reset@test.com", password = "NewPassword123!" });
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        await RegisterAsync(client, "badtoken@test.com");

        var reset = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { email = "badtoken@test.com", token = "not-a-real-token", newPassword = "NewPassword123!" });
        Assert.Equal(HttpStatusCode.BadRequest, reset.StatusCode);
    }
}
