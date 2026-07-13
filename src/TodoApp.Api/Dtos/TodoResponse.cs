namespace TodoApp.Api.Dtos;

// The shape of the data your API sends BACK to clients.
// It happens to mirror the Todo entity today, but keeping it separate means
// you can change your database table later without breaking API consumers.
public class TodoResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
