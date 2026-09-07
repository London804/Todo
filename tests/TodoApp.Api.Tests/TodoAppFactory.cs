using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoApp.Api.Data;

namespace TodoApp.Api.Tests;

// Boots the real API in memory for testing, but replaces the SQL Server database
// with EF Core's InMemory provider. That makes tests fast, isolated, and free of
// any dependency on a running SQL Server. WebApplicationFactory<Program> uses the
// public Program class we exposed in the API project.
public class TodoAppFactory : WebApplicationFactory<Program>
{
    // A unique name per factory instance means each test class gets its own
    // isolated in-memory store — no data bleeding between tests.
    private readonly string _dbName = $"TodoAppTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Run under the Development environment so the app starts cleanly.
        builder.UseEnvironment("Testing");

        // The JWT signing key lives in user secrets, which aren't loaded under the
        // Testing environment. Supply a test key (and issuer/audience) in memory so
        // token generation and validation work during tests.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-that-is-long-enough-for-hmac-sha256",
                ["Jwt:Issuer"] = "TodoApp.Tests",
                ["Jwt:Audience"] = "TodoApp.Tests",
                ["Jwt:ExpiryMinutes"] = "60",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server DbContext registration the app added in Program.cs.
            // In EF Core 9+ the provider is wired up via BOTH of these types, so both
            // must go — otherwise SqlServer and InMemory collide ("only a single
            // database provider can be registered").
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            // ...and replace it with the InMemory provider.
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
