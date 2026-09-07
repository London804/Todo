using Microsoft.AspNetCore.Identity;

namespace TodoApp.Api.Models;

// Our app's user. Inheriting from IdentityUser gives us all the standard fields
// (Id, UserName, Email, PasswordHash, etc.) managed by ASP.NET Core Identity.
// It's empty for now, but having our own type means we can add app-specific
// profile fields later without another big refactor.
public class ApplicationUser : IdentityUser
{
}
