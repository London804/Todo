using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TodoApp.Api;
using TodoApp.Api.Auth;
using TodoApp.Api.Data;
using TodoApp.Api.Models;

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
// The document transformer adds a JWT "Bearer" scheme so Swagger UI gets an
// "Authorize" button for testing protected endpoints.
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

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

// --- Authentication & Authorization ---------------------------------------

// ASP.NET Core Identity: manages users and password hashing, storing everything
// in our AppDbContext (which now inherits IdentityDbContext).
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Password rules enforced by UserManager.CreateAsync during registration.
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

// Bind the "Jwt" config section to JwtSettings, and register our token service.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddScoped<TokenService>();

// JWT bearer authentication: validates the "Authorization: Bearer <token>" header
// on incoming requests.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Configure the bearer options LAZILY from IOptions<JwtSettings> (rather than
// reading configuration eagerly here). This matters because it defers reading the
// signing key until the final merged configuration is in effect — important for
// tests, which inject their own Jwt settings after startup.
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
    {
        var jwt = jwtSettings.Value;
        // Keep claim names as-is (e.g. "sub" stays "sub") instead of the legacy
        // remapping to long XML claim URIs. Our controller reads "sub" directly.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };
    });

builder.Services.AddAuthorization();

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

// Order matters: authentication (who are you?) must run before authorization
// (are you allowed?). Both go after routing and before MapControllers.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Program is implicitly a class when using top-level statements, but it's
// 'internal' by default. Making it public (via this partial declaration) lets
// the test project reference it with WebApplicationFactory<Program>.
public partial class Program { }
