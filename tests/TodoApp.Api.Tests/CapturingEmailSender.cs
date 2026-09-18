using System.Collections.Concurrent;
using TodoApp.Api.Auth;

namespace TodoApp.Api.Tests;

// Test IEmailSender that records messages instead of sending them, so tests can
// read the reset link/token the app "emailed".
public class CapturingEmailSender : IEmailSender
{
    public record SentEmail(string To, string Subject, string Body);

    public ConcurrentBag<SentEmail> Sent { get; } = new();

    public Task SendAsync(string to, string subject, string body)
    {
        Sent.Add(new SentEmail(to, subject, body));
        return Task.CompletedTask;
    }
}
