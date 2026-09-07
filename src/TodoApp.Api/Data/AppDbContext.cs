using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Models;

namespace TodoApp.Api.Data;

// A DbContext is your gateway to the database. It represents a session with
// the DB and tracks changes to your entities so it knows what SQL to run.
//
// Inheriting from IdentityDbContext<ApplicationUser> (instead of plain DbContext)
// adds all the tables ASP.NET Core Identity needs — users, roles, claims, etc. —
// alongside our own Todos table.
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    // The options (which database, which connection string) are supplied by
    // Dependency Injection — see Program.cs where we call AddDbContext.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Each DbSet<T> maps to a table. "Todos" becomes the table name.
    // You'll query it like: _db.Todos.Where(t => !t.IsArchived)
    public DbSet<Todo> Todos => Set<Todo>();
}
