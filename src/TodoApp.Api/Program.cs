using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoApp.Api;
using TodoApp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Structured logging with Serilog. This replaces the default logging provider.
// Reading from configuration lets you tune levels/sinks in appsettings.json;
// FromLogContext enriches each log with contextual properties (e.g. request id).
builder.Services.AddSerilog((services, config) => config
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Global error handling: our handler turns unhandled exceptions into clean
// ProblemDetails responses, and AddProblemDetails() makes the framework use the
// same format for other errors too (e.g. 404s, validation 400s).
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register the database context. This tells EF Core to use SQL Server and
// where to find it (the "Default" connection string). Now any controller can
// ask for an AppDbContext in its constructor and DI will provide one.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// First in the pipeline so it wraps everything below and catches any exception
// they throw. Uses the GlobalExceptionHandler registered above.
app.UseExceptionHandler();

// Logs one structured summary line per HTTP request (method, path, status code,
// elapsed time) instead of the framework's noisier default per-request logging.
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Serves the OpenAPI document (the machine-readable API description)
    // at /openapi/v1.json.
    app.MapOpenApi();

    // Serves an interactive Swagger UI *page* at /swagger, pointed at the
    // document above. This is just the UI — it renders the OpenAPI doc that
    // MapOpenApi() already generates; it doesn't generate its own.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TodoApp API v1");
        options.RoutePrefix = "swagger"; // UI lives at /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Program is implicitly a class when using top-level statements, but it's
// 'internal' by default. Making it public (via this partial declaration) lets
// the test project reference it with WebApplicationFactory<Program>.
public partial class Program { }
