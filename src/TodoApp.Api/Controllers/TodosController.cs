using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Dtos;
using TodoApp.Api.Models;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // -> /api/todos
public class TodosController : ControllerBase
{
    // Dependency Injection in action: we ask for an AppDbContext in the
    // constructor, and the framework creates one (per request) and hands it in.
    // We never write "new AppDbContext()" ourselves.
    private readonly AppDbContext _db;

    public TodosController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/todos              -> active todos (not archived)
    // GET /api/todos?includeArchived=true -> everything
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoResponse>>> GetTodos(
        [FromQuery] bool includeArchived = false)
    {
        // Build a query. Nothing hits the database until ToListAsync() runs.
        var query = _db.Todos.AsQueryable();

        if (!includeArchived)
            query = query.Where(t => !t.IsArchived);

        // 'await' frees the thread to serve other requests while SQL Server works.
        var todos = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        // Map entities -> DTOs before returning (never expose entities directly).
        return Ok(todos.Select(ToResponse));
    }

    // GET /api/todos/5
    [HttpGet("{id:int}")] // the ":int" constraint means /api/todos/abc won't match
    public async Task<ActionResult<TodoResponse>> GetTodo(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo is null)
            return NotFound(); // 404

        return Ok(ToResponse(todo));
    }

    // POST /api/todos   { "title": "Buy milk" }
    [HttpPost]
    public async Task<ActionResult<TodoResponse>> CreateTodo(CreateTodoRequest request)
    {
        // Map the incoming DTO into a real entity, filling server-owned fields.
        var todo = new Todo
        {
            Title = request.Title,
            CreatedAt = DateTime.UtcNow, // server decides this, not the client
        };

        _db.Todos.Add(todo);      // stage the insert
        await _db.SaveChangesAsync(); // run it — SQL Server assigns todo.Id here

        // 201 Created + a Location header pointing at the new resource.
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, ToResponse(todo));
    }

    // PUT /api/todos/5   { "title": "Buy oat milk" }
    // PUT means "replace the editable state of this resource" — here, the title.
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TodoResponse>> UpdateTodo(int id, UpdateTodoRequest request)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo is null)
            return NotFound(); // 404 — can't edit something that doesn't exist

        todo.Title = request.Title;   // only the client-editable field changes
        await _db.SaveChangesAsync(); // EF Core detects the change and UPDATEs the row

        return Ok(ToResponse(todo));
    }

    // DELETE /api/todos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo is null)
            return NotFound();

        _db.Todos.Remove(todo);       // stage the delete
        await _db.SaveChangesAsync(); // run it — the row is gone from SQL Server

        // 204 No Content: success, and there's nothing meaningful to return.
        return NoContent();
    }

    // POST /api/todos/5/complete
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<TodoResponse>> CompleteTodo(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo is null)
            return NotFound();

        todo.IsCompleted = true;
        todo.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(); // EF Core detected the change and UPDATEs the row

        return Ok(ToResponse(todo));
    }

    // POST /api/todos/5/archive
    [HttpPost("{id:int}/archive")]
    public async Task<ActionResult<TodoResponse>> ArchiveTodo(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo is null)
            return NotFound();

        todo.IsArchived = true;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(todo));
    }

    // A tiny helper to convert a Todo entity into a TodoResponse DTO.
    private static TodoResponse ToResponse(Todo t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        IsCompleted = t.IsCompleted,
        IsArchived = t.IsArchived,
        CreatedAt = t.CreatedAt,
        CompletedAt = t.CompletedAt,
    };
}
