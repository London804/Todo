namespace TodoApp.Api.Auth;

// Development email sender: instead of actually sending mail, it logs the message
// (including any reset link) to the console. This lets you test the flow locally
// without configuring a real email provider. Swap this registration in Program.cs
// for a real IEmailSender in production.
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("DEV EMAIL → {To}\nSubject: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
