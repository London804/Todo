using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.Dtos;

// The shape of the data a client sends when EDITING a todo.
// Like CreateTodoRequest, it only exposes fields the client is allowed to change.
// Server-owned fields (Id, CreatedAt) and workflow flags (IsCompleted via the
// /complete endpoint, IsArchived via /archive) are deliberately not here.
public class UpdateTodoRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be 1-200 characters.")]
    public string Title { get; set; } = string.Empty;
}
