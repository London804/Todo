using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api;

// Catches any unhandled exception that escapes a controller and turns it into a
// clean, structured JSON response (RFC 7807 "ProblemDetails") instead of leaking
// a stack trace. Registered in Program.cs via AddExceptionHandler + UseExceptionHandler.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    // Return true if we handled the response; false lets the next handler try.
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the *full* detail server-side — this is where the stack trace belongs,
        // not in the HTTP response the client sees.
        _logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);

        // Send the client a generic, safe 500 with no internal details.
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        };

        httpContext.Response.StatusCode = problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true; // we've written the response; stop the pipeline here
    }
}
