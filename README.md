# TodoApp

A .NET 10 Web API for managing todos, backed by SQL Server via EF Core.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running **SQL Server** instance (local install, or Docker — see below)
- EF Core CLI tools (for migrations):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Running SQL Server in Docker (optional)

If you don't have SQL Server locally, you can run it in a container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

## Setup

The database connection string is **not** stored in `appsettings.json` — it's kept
out of source control in [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).
Set yours once (replace the password with your own):

```bash
cd src/TodoApp.Api
dotnet user-secrets set "ConnectionStrings:Default" \
  "Server=localhost,1433;Database=TodoApp;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=True"
```

### JWT signing key

Authentication uses JWTs signed with a secret key, also kept in user secrets.
Generate a strong random key and store it (non-secret JWT settings — issuer,
audience, expiry — live in `appsettings.json`):

```bash
cd src/TodoApp.Api
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
```

### Create the database

Apply the EF Core migrations to create the `TodoApp` database and its tables:

```bash
dotnet ef database update --project src/TodoApp.Api
```

## Running the app

From the repository root:

```bash
# HTTP only — listens on http://localhost:5263
dotnet run --project src/TodoApp.Api

# HTTP + HTTPS — adds https://localhost:7243
dotnet run --project src/TodoApp.Api --launch-profile https
```

The first time you use the HTTPS profile, trust the dev certificate:

```bash
dotnet dev-certs https --trust
```

### URLs

| What | URL |
|------|-----|
| API base | `http://localhost:5263` / `https://localhost:7243` |
| Swagger UI | `/swagger` (Development only) |
| OpenAPI document | `/openapi/v1.json` |

> Note: there is no page at the root `/` — it's an API. Hit an endpoint like
> `/api/todos` or open Swagger UI.

> **Ports:** `5263`/`7243` aren't .NET defaults — they're random ports assigned
> to this project at scaffold time and stored in
> `src/TodoApp.Api/Properties/launchSettings.json`. Edit that file to change them.
> It's a dev-only convenience; in production the port comes from `ASPNETCORE_URLS`,
> `--urls`, or a reverse proxy instead.

## Authentication

The API uses JWT bearer authentication. All `/api/todos` endpoints require a valid
token, and todos are scoped per user — you only ever see your own.

1. **Register** or **log in** to get a token:
   ```bash
   curl -k -X POST https://localhost:7243/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"email":"me@example.com","password":"Password123!"}'
   # -> { "token": "eyJ...", "email": "me@example.com" }
   ```
2. **Send the token** on every todos request:
   ```bash
   curl -k https://localhost:7243/api/todos \
     -H "Authorization: Bearer eyJ..."
   ```

### Using a token in Swagger UI

1. Expand `POST /api/auth/register` (or `/login`) → **Try it out** → fill in the
   body → **Execute**.
2. Copy the `token` value from the response (just the long string, not the quotes).
3. Click **Authorize** (top-right), paste the token into the **Value** field,
   then **Authorize** → **Close**. Protected endpoints now work.

> **Paste only the raw token — do _not_ prefix it with `Bearer `.** The scheme is
> declared as `type: http, scheme: bearer`, so Swagger adds the `Bearer ` prefix
> for you. Pasting `Bearer eyJ...` yourself produces a doubled prefix and a `401`.

The token in Swagger is remembered for the page session; refreshing the page
clears it, so you'd re-Authorize. Tokens expire after `ExpiryMinutes` (default 60),
after which you log in again for a fresh one.

### Password reset & email (Mailpit)

Email is sent over SMTP (MailKit). Locally, a **Mailpit** container catches it and
shows it in a web inbox — no real email leaves your machine. Start it once:

```bash
docker run -d --name todo-mailpit -p 1025:1025 -p 8025:8025 axllent/mailpit
```

Then use **Forgot password** in the app and open the inbox at
**http://localhost:8025** to click the reset link. SMTP settings live in the
`Email` config section (dev defaults point at Mailpit: `localhost:1025`, no TLS,
no credentials).

**In production:** point the same `Email` config at one outbound mail provider your
app uses to send (SendGrid/Mailgun/SES/SMTP): set `Host`/`Port`, `UseStartTls:
true`, and put **your app's provider credentials** in secrets — i.e.
`Username`/`Password` are your application's login (often `apikey` + an API key) to
that one sending service, **not** any recipient's email account. Your app connects
to that one provider, which then delivers to each recipient's own email host. These
credentials can send mail as your domain, so they belong in secrets/env vars/a key
vault — never in `appsettings.json`. The code doesn't change between dev and prod —
only config. The reset link points at `Frontend:BaseUrl`.

## API endpoints

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| POST | `/api/auth/register` | — | Create an account, returns a JWT |
| POST | `/api/auth/login` | — | Log in, returns a JWT |
| POST | `/api/auth/forgot-password` | — | Email a password-reset link (always returns 200) |
| POST | `/api/auth/reset-password` | — | Set a new password using the emailed token |
| GET | `/api/todos` | ✔ | List active todos (`?includeArchived=true` for all) |
| GET | `/api/todos/{id}` | ✔ | Get one todo |
| POST | `/api/todos` | ✔ | Create — body: `{ "title": "..." }` |
| PUT | `/api/todos/{id}` | ✔ | Edit a todo's title |
| DELETE | `/api/todos/{id}` | ✔ | Delete a todo |
| POST | `/api/todos/{id}/complete` | ✔ | Mark complete |
| POST | `/api/todos/{id}/archive` | ✔ | Archive (soft-hide) |

## Running the tests

```bash
dotnet test
```

Integration tests (xUnit) spin up the API in memory with an EF Core InMemory
database, so they don't require a running SQL Server.

## Project structure

```
src/TodoApp.Api/        The Web API
  Controllers/          HTTP endpoints (Todos, Auth)
  Models/               EF Core entities (Todo, ApplicationUser)
  Dtos/                 Request/response contracts
  Data/                 AppDbContext (IdentityDbContext)
  Auth/                 JWT settings, token service, Swagger bearer scheme
  Validation/           Custom validation attributes
  Migrations/           EF Core migrations
tests/TodoApp.Api.Tests/  Integration tests
```

## Notes

- **Logging:** structured logging via Serilog; one summary line per request to the console.
- **Errors:** unhandled exceptions return a clean `ProblemDetails` 500; full detail is logged server-side.
- **Times** are stored and returned in **UTC**.
