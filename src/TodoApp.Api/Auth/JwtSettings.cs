namespace TodoApp.Api.Auth;

// Strongly-typed view of the "Jwt" section in configuration. Issuer/Audience/
// ExpiryMinutes live in appsettings.json; the signing Key is a secret and is
// kept in user secrets (dev) or environment variables (prod), never in source.
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
