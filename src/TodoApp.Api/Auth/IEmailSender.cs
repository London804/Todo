namespace TodoApp.Api.Auth;

// Abstraction over sending an email. Swapping the implementation (dev logger vs. a
// real SMTP/SendGrid/etc. sender) doesn't touch the controllers that use it.
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}
