using System.ComponentModel.DataAnnotations;
using TodoApp.Api.Validation;

namespace TodoApp.Api.Dtos;

// The shape of the data a client sends when CREATING a todo.
// Note it only has Title — clients can't set Id, CreatedAt, IsCompleted, etc.
// The [ApiController] attribute makes these validation rules automatic:
// a bad request gets a 400 response before your code even runs.
public class CreateTodoRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be 1-200 characters.")]
    [NotWhitespace(ErrorMessage = "Title cannot be empty or whitespace.")]
    public string Title { get; set; } = string.Empty;
}
