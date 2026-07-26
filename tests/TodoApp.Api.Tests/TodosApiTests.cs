using System.Net;
using System.Net.Http.Json;
using TodoApp.Api.Dtos;

namespace TodoApp.Api.Tests;

// Integration tests: each test makes real HTTP calls against the app running
// in memory (via TodoAppFactory), exercising routing, validation, the controller,
// and EF Core end to end.
//
// xUnit creates a fresh instance of this class for every test method, so building
// the factory in the constructor gives each test its own isolated in-memory DB.
public class TodosApiTests : IDisposable
{
    private readonly TodoAppFactory _factory;
    private readonly HttpClient _client;

    public TodosApiTests()
    {
        _factory = new TodoAppFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // Small helper: create a todo and return the deserialized response.
    private async Task<TodoResponse> CreateTodoAsync(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoResponse>())!;
    }

    [Fact]
    public async Task CreateTodo_ReturnsCreatedWithBody()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(todo);
        Assert.Equal("Buy milk", todo!.Title);
        Assert.True(todo.Id > 0);
        Assert.False(todo.IsCompleted);
        Assert.False(todo.IsArchived);
        Assert.Null(todo.CompletedAt);
        // 201 responses should carry a Location header pointing at the new resource.
        Assert.NotNull(response.Headers.Location);
    }

    [Theory]
    [InlineData("")]      // empty
    [InlineData("   ")]   // whitespace only
    [InlineData("\t\n")]  // other whitespace
    public async Task CreateTodo_WithBlankTitle_ReturnsBadRequest(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_ReturnsCreatedTodos()
    {
        await CreateTodoAsync("first");
        await CreateTodoAsync("second");

        var todos = await _client.GetFromJsonAsync<List<TodoResponse>>("/api/todos");

        Assert.NotNull(todos);
        Assert.Equal(2, todos!.Count);
    }

    [Fact]
    public async Task GetTodoById_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/todos/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodo_ChangesTitle()
    {
        var created = await CreateTodoAsync("original");

        var response = await _client.PutAsJsonAsync($"/api/todos/{created.Id}", new { title = "edited" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.Equal("edited", updated!.Title);
        Assert.Equal(created.Id, updated.Id);
    }

    [Theory]
    [InlineData("")]      // empty
    [InlineData("   ")]   // whitespace only
    public async Task UpdateTodo_WithBlankTitle_ReturnsBadRequest(string title)
    {
        var created = await CreateTodoAsync("original");
        var response = await _client.PutAsJsonAsync($"/api/todos/{created.Id}", new { title });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodo_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/todos/999999", new { title = "x" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_RemovesIt()
    {
        var created = await CreateTodoAsync("to delete");

        var delete = await _client.DeleteAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        // It should no longer be retrievable.
        var get = await _client.GetAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/todos/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompleteTodo_SetsCompletedFlags()
    {
        var created = await CreateTodoAsync("finish me");

        var response = await _client.PostAsync($"/api/todos/{created.Id}/complete", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.True(todo!.IsCompleted);
        Assert.NotNull(todo.CompletedAt);
    }

    [Fact]
    public async Task ArchiveTodo_HidesFromDefaultListButShowsWhenIncluded()
    {
        var created = await CreateTodoAsync("archive me");

        var archive = await _client.PostAsync($"/api/todos/{created.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);

        // Default list excludes archived todos.
        var activeList = await _client.GetFromJsonAsync<List<TodoResponse>>("/api/todos");
        Assert.DoesNotContain(activeList!, t => t.Id == created.Id);

        // ...but includeArchived=true brings it back.
        var allList = await _client.GetFromJsonAsync<List<TodoResponse>>("/api/todos?includeArchived=true");
        Assert.Contains(allList!, t => t.Id == created.Id);
    }
}
