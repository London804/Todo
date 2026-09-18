using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TodoApp.Api.Auth;

// Sends email over SMTP using MailKit (the recommended .NET email library). Works
// against the local Mailpit container in dev and real providers in production —
// the difference is entirely in EmailSettings (host, TLS, credentials).
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();

        // Mailpit uses a plain connection; real providers use STARTTLS.
        var socketOptions = _settings.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;
        await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);

        // Authenticate only when credentials are configured (Mailpit needs none).
        if (!string.IsNullOrEmpty(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
