# JWT Authentication in .NET 10 Web API: Secure It Before AI Touches It

Source code for the Core Knowledge Sharing video. The ProductsApi from the previous video (ASP.NET Core Web API, .NET 10, EF Core, PostgreSQL, and an MCP endpoint for AI) locked down with **ASP.NET Core Identity + JWT**:

- users, roles and password hashes in the same PostgreSQL database (Identity on EF Core)
- `POST /api/auth/login` returns a **15-minute JWT access token** and a **7-day refresh token**
- refresh tokens are stored as a SHA-256 hash and **rotated**: each one works only once
- `[Authorize]` on `ProductsController`, and **only the Admin role can delete**
- the MCP endpoint is locked too: `app.MapMcp("/mcp").RequireAuthorization()`
- Claude Code sends the token with a `headers` entry in `.mcp.json`

Previous video (the MCP server this builds on): https://youtu.be/UZC43eK3KZ8

## Endpoints

| Endpoint | Who | Result |
|---|---|---|
| `POST /api/auth/register` | anyone | creates a user with the `User` role |
| `POST /api/auth/login` | anyone | `{ accessToken, expiresAt, refreshToken }`, or 401 |
| `POST /api/auth/refresh` | holder of a refresh token | a new pair; the old refresh token is revoked |
| `GET /api/products`, `GET /api/products/{id}`, `POST`, `PUT` | any logged-in user | as before |
| `DELETE /api/products/{id}` | `Admin` only | 204; a normal user gets 403 |
| `/mcp` | any logged-in user | the three read-only MCP tools |

## Run it

Requirements: .NET 10 SDK and PostgreSQL.

1. Secrets go in user secrets, never in `appsettings.json`:
   ```bash
   dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
   dotnet user-secrets set "Seed:AdminPassword" "<an admin password>"
   ```
   The admin password must follow Identity's default rules (6+ characters, upper case, lower case, a digit and a symbol).
2. Pass your real database password through an environment variable (`appsettings.json` only has a placeholder):
   ```bash
   export ConnectionStrings__Default="Host=localhost;Port=5432;Database=productsauthdb;Username=postgres;Password=<your password>"
   ```
   (PowerShell: `$env:ConnectionStrings__Default = '...'`)
3. `dotnet run`. On first start the database `productsauthdb` is created with the products, the roles `Admin` and `User`, and the account `admin@demo.local` (`EnsureCreated`, demo only; use migrations in a real project). It must be a new database: `EnsureCreated` does nothing on a database that already exists.

## Try it with curl (Git Bash)

```bash
pick() { grep -o "\"$1\":\"[^\"]*" | cut -d'"' -f4; }     # or use jq
AI_USER="{\"email\":\"claude@demo.local\",\"password\":\"$AI_PASSWORD\"}"

curl -i localhost:5080/api/products                         # 401
curl -i localhost:5080/api/auth/register --json "$AI_USER"  # 200
TOKEN=$(curl -s localhost:5080/api/auth/login --json "$AI_USER" | pick accessToken)
curl -s localhost:5080/api/products -H "Authorization: Bearer $TOKEN"                   # 200
curl -i -X DELETE localhost:5080/api/products/8 -H "Authorization: Bearer $TOKEN"     # 403
```

To watch a token expire without waiting 15 minutes: `Jwt__AccessTokenMinutes=1 dotnet run`. `ClockSkew` is set to 30 seconds (the default is 5 minutes), so the token is rejected about 90 seconds after login, with `WWW-Authenticate: Bearer error="invalid_token", error_description="The token expired at '...'"`.

## Connect Claude Code

`.mcp.json` sends the token in a header. The value comes from an environment variable, so the token is never saved in the file:

```json
"headers": { "Authorization": "Bearer ${PRODUCTS_API_TOKEN}" }
```

```bash
export PRODUCTS_API_TOKEN=$(curl -s localhost:5080/api/auth/login --json "$AI_USER" | pick accessToken)
claude --tools ""
```

Give the AI a **User** token, never an Admin token. The access token lasts 15 minutes; after that, log in again and restart Claude Code. `--tools ""` needs a shell that passes an empty argument: Git Bash, cmd or PowerShell 7.

## Using GitHub Copilot in VS Code

`.vscode/mcp.json` uses an `inputs` entry with `"password": true` and `"Authorization": "Bearer ${input:products-token}"`. VS Code asks for the token when the server starts, hides it and stores it.

## Demo vs production

A bearer token in a header is the simple path, good for learning, local tools and scripts. For production, the MCP specification defines an OAuth-based authorization flow (the server publishes Protected Resource Metadata, the client finds the authorization server, the user logs in in the browser, and the client keeps the tokens fresh). Claude Code and VS Code both support it; it needs an authorization server.

## Versions

.NET 10 · Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.13 · Microsoft.AspNetCore.Authentication.JwtBearer 10.0.13 · ModelContextProtocol.AspNetCore 2.2.0 · Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0

Created by Ankush Agnihotri · ankush1802@outlook.com
