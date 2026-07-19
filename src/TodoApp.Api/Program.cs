using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the database context. This tells EF Core to use SQL Server and
// where to find it (the "Default" connection string). Now any controller can
// ask for an AppDbContext in its constructor and DI will provide one.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

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
