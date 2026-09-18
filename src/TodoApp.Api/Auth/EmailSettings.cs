namespace TodoApp.Api.Auth;

// SMTP configuration, bound from the "Email" config section. In dev these point at
// the local Mailpit container (no auth, no TLS). In production, point them at a
// real provider (SendGrid/Mailgun/SES/etc.): set Host/Port, UseStartTls = true,
// and put Username/Password in secrets. The code doesn't change — only config.
public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string FromAddress { get; set; } = "noreply@todoapp.local";
    public string FromName { get; set; } = "TodoApp";

    // Left empty for Mailpit (which accepts unauthenticated mail); set in prod.
    public string? Username { get; set; }
    public string? Password { get; set; }

    // Mailpit: false. Most real providers: true (STARTTLS on submission port 587).
    public bool UseStartTls { get; set; }
}
