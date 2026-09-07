namespace TodoApp.Api.Models;

// This class is an "entity" — EF Core maps it to a database table.
// Each property becomes a column; each instance becomes a row.
public class Todo
{
    // 'Id' is a convention EF Core recognizes as the primary key.
    // For an int key, SQL Server auto-generates the value (IDENTITY).
    public int Id { get; set; }

    // The task's text. Defaulting to "" avoids null-reference warnings.
    public string Title { get; set; } = string.Empty;

    // Completing a task flips this to true.
    public bool IsCompleted { get; set; }

    // Archiving hides a task without deleting it.
    public bool IsArchived { get; set; }

    // When the task was created. Stored in UTC (best practice for backends).
    public DateTime CreatedAt { get; set; }

    // Nullable (DateTime?) because an incomplete task has no completion time yet.
    public DateTime? CompletedAt { get; set; }

    // The id of the ApplicationUser who owns this todo. Set by the server from
    // the authenticated user's token — never from client input. Every query is
    // filtered by this so users only ever see their own todos.
    public string UserId { get; set; } = string.Empty;
}
