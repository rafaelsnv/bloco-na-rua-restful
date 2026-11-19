<!-- Context: project-intelligence/technical | Priority: critical | Version: 4.2 | Updated: 2026-11-18 -->

# Technical Domain

> BlocoNaRua is a .NET 8 RESTful API. This file is the canonical reference for its stack, architecture, and conventions.
> Companion project (Flutter mobile client) lives in a separate repo and is not covered here.

## Quick Reference

- **Update Triggers**: Tech stack change, new pattern, architecture pivot, new project added to the solution, auth/identity change (see `identity-auth.md`).
- **Version**: 4.1 — auth migrated to API-issued HS256 JWT + Google OAuth (Supabase Auth removed, 2026-11-18); soft delete, CancellationToken, EF error logging documented.

---

## Primary Stack

### Backend (.NET 8)

| Layer | Technology | Version | Rationale |
|-------|------------|---------|-----------|
| Runtime | .NET | 8.0 (LTS) | Cross-platform, long support window |
| Language | C# | 12 | Primary constructors, collection expressions |
| Web | ASP.NET Core | 8.0 | `[ApiController]`, minimal hosting, OpenAPI via Swashbuckle |
| Auth | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) | 8.0 | Validates API-issued HS256 tokens (self-issued, no external IdP) |
| OAuth | `Microsoft.AspNetCore.Authentication.Google` | 8.0.11 | Google OAuth 2.0 social login (challenge → complete → token pair) |
| ORM | EF Core + Npgsql | 9.0.6 / 9.0.4 | Postgres + enum mapping + LINQ + global query filters |
| DB | PostgreSQL (self-hosted) | 18 | Dokploy docker-compose; snake_case columns |
| Docs | Swashbuckle.AspNetCore | 6.6.2 | `/swagger` JSON everywhere, Swagger UI in Development |
| Versioning | Asp.Versioning.Mvc | 8.1.0 | URL segment versioning (`/api/v1/...`) |
| Cache | `Microsoft.Extensions.Caching.Memory` | (built-in) | In-process, 5 min TTL |

### Tests

| Tool | Version | Use |
|------|---------|-----|
| xUnit | 2.5.3 | Test runner + assertions |
| Moq | 4.20.72 | Controller / service mocks |
| EF Core InMemory | 9.0.6 | Service tests against a fake `DbContext` |
| coverlet.collector | 6.0.0 | Coverage reporting |

> No Flutter code lives in this repo. The companion Flutter app (mobile) is a separate repo and consumes this API.

---

## Solution Layout

```
BlocoNaRua.sln
├── BlocoNaRua.Core           # Abstractions: EntityBase, IRepositoryBase<T>
├── BlocoNaRua.Domain         # POCOs: entities + enums
├── BlocoNaRua.Data           # EF Core DbContext, mappings, repositories
├── BlocoNaRua.Services       # Business rules, per-block role checks, caching, error logging
├── BlocoNaRua.Restful        # ASP.NET host: controllers, DTOs, mappers, middleware, Program.cs
└── BlocoNaRua.Tests          # xUnit + Moq + InMemory
```

Dependency direction (forbidden edges in **bold**):
```
Restful  →  Services  →  Data  →  Domain  →  Core
                                   ↑
                           (no path back)
```
**`Data` must not reference `Services`** and **`Domain` must not reference `Data`/`Services`/`Restful`**.

---

## API Conventions

### Route shape

```csharp
[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class MeetingsController(IMeetingService service, IMemberIdentityService memberIdentityService) : ControllerBase
```

- URL: `api/v1/<ControllerName>` (PascalCase; singular names match entity names: `Members`, `Meetings`, `CarnivalBlocks`, `CarnivalBlockMembers`, `MeetingPresences`).
- All controllers inherit `ControllerBase` (not `Controller`) — no view support needed.
- Versioned via `AddApiVersioning` with `AssumeDefaultVersionWhenUnspecified = true` and `SubstituteApiVersionInUrl = true`.
- Anonymous endpoints are the auth routes: `POST /api/v1/auth/login`, `POST /api/v1/auth/signup`, `GET /api/v1/auth/google/challenge`, `GET /api/v1/auth/google/complete`, and `POST /api/v1/auth/refresh`. Every other controller carries `[Authorize]`, and `AdminController` additionally requires `[Authorize(Policy = "AdminOnly")]`.

### HTTP semantics

| Verb   | Purpose | Success | Failure |
|--------|---------|---------|---------|
| GET    | List / detail | 200 | 404 |
| POST   | Create | 201 + `Location` | 400 / 404 / 401 |
| PUT    | Update | 200 | 404 / 401 |
| DELETE | Remove (soft) | 204 | 404 / 401 |

Exception → status mapping (in every controller):
```csharp
catch (KeyNotFoundException ex)      => NotFound(ex.Message);
catch (UnauthorizedAccessException ex) => Unauthorized(ex.Message);
catch (InvalidOperationException)    => BadRequest(ex.Message);
```

### Identity and authorization (summary)

The caller is identified by an **API-issued JWT (HS256)** sent as `Authorization: Bearer <token>`. Social login is Google OAuth 2.0 (`GET /api/v1/auth/google/challenge` → `GET /api/v1/auth/google/complete` returns the token pair). The JWT `sub` claim is the member's UUID, which `IMemberIdentityService` resolves to the `Members` row and its `int` id. Platform-level access uses the `AdminOnly` policy (`role: admin` claim). Block-level roles (`Owner` / `Manager` / `Member`) are checked inside the service layer.

Full flow (Google challenge/complete, JWT validation parameters, identity lookup, test bypass): see `identity-auth.md`.

Key code locations:
- Key rotation: `Auth:SigningKey` (env-only, ≥ 32 bytes) — see `identity-auth.md` §3.
- JWT validation + `AdminOnly` policy: `BlocoNaRua.Restful/Program.cs` (lines 85-129)
- Identity lookup: `BlocoNaRua.Services/Implementations/MemberIdentityService.cs`, `BlocoNaRua.Services/Interfaces/IMemberIdentityService.cs`
- Admin routes: `BlocoNaRua.Restful/Controllers/AdminController.cs`

### DTOs (records)

```csharp
namespace BlocoNaRua.Restful.Models.Xxx;

public record XxxCreate(string Name, ...);          // POST body
public record XxxUpdate(string Name, ...);          // PUT body
public record XxxResponse(int Id, string Name, ...); // Response
```
- Stored under `BlocoNaRua.Restful/Models/<Entity>/`.
- Validation via `[EmailAddress]`, `[Phone]`, `[RegularExpression]` attributes on records (see `MemberCreate`).

### Mappers (static, one file per entity)

```csharp
public static class XxxMapper
{
    public static XxxResponse ToDTO(this XxxEntity entity) => new(entity.Id, ...);
}
```
- `XxxController.GetById` returns `Ok(entity.ToDTO())`.
- `GetAll` returns `Ok(list.Select(x => x.ToDTO()))`.

---

## Component Patterns

### Entity (POCO with primary constructor)

```csharp
public class XxxEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public Guid Uuid { get; set; } = new();
    public bool IsActive { get; set; } = true;   // soft-delete flag, see "Soft Delete"
    public ICollection<YyyEntity> Yyys { get; set; } = [];

    public XxxEntity(int id, string name, Guid uuid) : this(id)
    {
        Name = name;
        Uuid = uuid;
    }
}
```
- `EntityBase` (`BlocoNaRua.Core/Models/EntityBase.cs`) provides `Id` (private setter), `CreatedAt`, and `UpdatedAt`. `IsActive` is declared on each concrete entity, not on the base.
- `string = string.Empty` / `ICollection<T> = []` to satisfy nullable refs.

### Repository (generic base + concrete overrides)

```csharp
public class MeetingsRepository(AppDbContext appContext) : RepositoryBase<MeetingEntity>(appContext), IMeetingsRepository
{
    public async Task<IList<MeetingEntity>> GetByBlockIdsAsync(IList<int> blockIds, CancellationToken ct) =>
        await DbSet.Where(m => blockIds.Contains(m.CarnivalBlockId)).ToListAsync(ct);
}
```
- `RepositoryBase<TEntity>` (`BlocoNaRua.Data/Repositories/Base/RepositoryBase.cs`) provides the generic CRUD methods.
- Concrete repositories add their own methods, each taking `CancellationToken ct` as the last parameter (see "CancellationToken Convention").
- Read queries use `DbSet.AsNoTracking()` by default. Global query filters handle `IsActive` (see "Soft Delete").

### Service (primary constructor + cache + inline role check)

```csharp
public class MeetingService
(
    IMeetingsRepository repository,
    ICarnivalBlocksRepository carnivalBlocksRepository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository,
    IMeetingPresencesRepository meetingPresencesRepository,
    IMemoryCache cache
) : IMeetingService
{
    private readonly IMeetingsRepository _repository = repository;
    private readonly IMemoryCache _cache = cache;
    // ... other fields assigned the same way

    private async Task<RolesEnum?> GetMemberRoleInline(int carnivalBlockId, int memberId)
    {
        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");
        if (carnivalBlock.OwnerId == memberId)
            return RolesEnum.Owner;
        return await _carnivalBlockMembersRepository.GetMemberRole(carnivalBlockId, memberId, CancellationToken.None);
    }

    public async Task<MeetingEntity?> GetByIdAsync(int id)
    {
        var cacheKey = $"Meeting_{id}";
        if (_cache.TryGetValue(cacheKey, out MeetingEntity? meeting)) return meeting;

        meeting = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (meeting != null) _cache.Set(cacheKey, meeting, TimeSpan.FromMinutes(5));
        return meeting;
    }
}
```
- Writes that touch a block take the acting `int loggedMember` (the memberId resolved from the JWT by the controller) and call the private `GetMemberRoleInline` helper. Denial throws `UnauthorizedAccessException`.
- Every service keeps its own private `GetMemberRoleInline`. There is no shared authorization service; see "Role-Based Authorization".
- All write methods call `_cache.Remove(...)` for affected keys.

### Controller (DI + identity + try/catch)

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] MeetingUpdate model)
{
    var memberId = await _memberIdentityService.GetMemberIdAsync();
    var entity = new MeetingEntity(id: 0, name: model.Name, description: model.Description,
        location: model.Location, meetingCode: string.Empty,
        meetingDateTime: model.MeetingDateTime, carnivalBlockId: 0);
    try
    {
        var updated = await _service.UpdateAsync(id, entity, memberId);
        if (updated is null) return NotFound();
        return Ok(updated.ToDTO());
    }
    catch (KeyNotFoundException ex)     { return NotFound(ex.Message); }
    catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
}
```
- The acting member id always comes from `IMemberIdentityService.GetMemberIdAsync()`. Never take it from a request header, query string, or body.
- Controllers do not yet forward `HttpContext.RequestAborted` (see "CancellationToken Convention").

---

## Domain Model (5 entities)

```
Member ──┬── CarnivalBlockMembers ── CarnivalBlock ── Meeting
         │                              │              │
         └────── MeetingPresence ───────┘──────────────┘
```

- `Member` — person (registered via Google login → token issuance, or `POST /api/v1/Members`). Has `Guid Uuid` (unique index), which is the local identity anchor — the JWT `sub` claim equals it.
- `CarnivalBlock` — the "bloco" itself. Has `OwnerId`, two invite codes (`InviteCode`, `ManagersInviteCode`), and an image URL.
- `CarnivalBlockMembers` — junction with `Role` enum (`Member` / `Manager` / `Owner`).
- `Meeting` — an event belonging to a block. Has `MeetingCode` (6 chars).
- `MeetingPresence` — attendance record. Triple FK (`MemberId` + `MeetingId` + `CarnivalBlockId`), `IsPresent` boolean.

Every entity carries `IsActive` (default `true`). See "Soft Delete".

Database column naming is `snake_case` via `HasColumnName("invite_code")` etc.

---

## Soft Delete

Hard deletes are not used. Rows are deactivated, which keeps attendance and block history queryable.

**Entity flag** — every entity has `public bool IsActive { get; set; } = true;` and is mapped as `is_active` (`IsRequired()`, default `true`) in its `Data/Mapping/*Configuration.cs`.

**Global query filter** — each of the 5 entity configurations calls `builder.HasQueryFilter(x => x.IsActive)`. EF Core applies this filter to every query automatically, so inactive rows never reach services. Do not bypass it with `IgnoreQueryFilters()` unless the code is explicitly reading history, and document why.

**Repository delete** — each repository's `DeleteAsync` sets `IsActive = false` and `UpdatedAt`, saves, and clears the change tracker. It does not call `Remove`.

**Cascade** — parent deletes cascade through explicit `SoftDeleteBy…Async` repository methods, called from the service that owns the delete:

| Service delete | Cascades to |
|----------------|-------------|
| `CarnivalBlockService.DeleteAsync` | `MeetingPresences.SoftDeleteByBlockIdAsync`, `Meetings.SoftDeleteByBlockIdAsync`, `CarnivalBlockMembers.SoftDeleteByBlockIdAsync` |
| `MeetingService.DeleteAsync` | `MeetingPresences.SoftDeleteByMeetingIdAsync` |
| `MembersService.DeleteAsync` | `CarnivalBlockMembers.SoftDeleteByMemberIdAsync`, `MeetingPresences.SoftDeleteByMemberIdAsync` |

The cascade methods filter on `IsActive` before updating (`Where(... && p.IsActive)`), so they only touch live rows.

> Note: `MembersRepository` and `CarnivalBlocksRepository` deactivate only their own row. Their cascades run from the services above. The cascade methods live in `MeetingsRepository`, `MeetingPresencesRepository`, and `CarnivalBlockMembersRepository`.

Rules for new code:
- New entities get `IsActive`, a `HasQueryFilter`, and an `is_active` column mapping.
- New deletes set `IsActive = false`. Do not call `Remove`.
- A new parent-child relationship needs a `SoftDeleteBy…Async` method on the child repository and a call from the parent's service.

Decision record: `lookup/decisions.md` ("Soft Delete via `IsActive` Flag", 2026-10-20, commit `a5311c9`).

---

## CancellationToken Convention

`CancellationToken` is part of the repository contract:
- Every repository method added in the current code takes `CancellationToken ct` as its last parameter and passes it to EF Core (`ToListAsync(ct)`, `FindAsync([id], ct)`, `SaveChangesAsync(ct)`).
- Services call repositories with `CancellationToken.None` today. Services and controllers do not yet accept or forward a token.

Rules for new code:
- New repository methods take `CancellationToken ct` as the last parameter and forward it.
- New service methods that do I/O should accept `CancellationToken ct` and forward it. Do not drop it at the boundary.
- Controllers should pass `HttpContext.RequestAborted` once services accept tokens. This is planned, not done.

Decision record: `lookup/decisions.md` ("Remove Custom Authorization Service… + `CancellationToken`", 2026-07-25).

---

## Role-Based Authorization

Platform-level access is the `AdminOnly` ASP.NET policy (see `identity-auth.md`). Block-level access is checked in each service with a private helper. It is not an ASP.NET policy, because policies do not know the block id.

```csharp
private async Task<RolesEnum?> GetMemberRoleInline(int carnivalBlockId, int memberId)
{
    var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockId, CancellationToken.None)
        ?? throw new KeyNotFoundException("Carnival block does not exist.");
    if (carnivalBlock.OwnerId == memberId)
        return RolesEnum.Owner;
    return await _carnivalBlockMembersRepository.GetMemberRole(carnivalBlockId, memberId, CancellationToken.None);
}
```

Usage in a write method:
```csharp
var memberRole = await GetMemberRoleInline(entity.CarnivalBlockId, loggedMember);
if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
    throw new UnauthorizedAccessException("Member is not authorized to update this meeting.");
```

Rules by operation (the `Meeting` row is verified against `MeetingService`; the other rows are not yet re-verified against code):

| Operation | Required role |
|-----------|---------------|
| Update / Delete `CarnivalBlock` | `Owner` (for Delete) or `Owner`/`Manager` (for Update) |
| Add / Update / Remove `CarnivalBlockMembers` | `Owner` or `Manager` |
| Create / Update / Delete `Meeting` | `Owner` or `Manager` (of the parent block) |
| Create / Update / Delete `MeetingPresence` | Self (`MemberId == loggedMember`) OR `Owner`/`Manager` |
| Update / Delete own `Member` profile | Self only (`id == loggedMember`) |

Why not a shared service: a shared authorization service was removed in commit `0581d26`. It had a single usage pattern, so the indirection cost more than it saved. Role rules now sit next to the business rules they protect (see `lookup/decisions.md`).

---

## Caching

| Key | Type | TTL | Invalidated by |
|-----|------|-----|----------------|
| `CarnivalBlocks_All` | `IList<CarnivalBlockEntity>` | 5 min | CarnivalBlock create/update/delete |
| `CarnivalBlock_{id}` | `CarnivalBlockEntity` | 5 min | CarnivalBlock update/delete |
| `Meetings_All` | `IList<MeetingEntity>` | 5 min | Meeting create/update/delete |
| `Meeting_{id}` | `MeetingEntity` | 5 min | Meeting update/delete |
| `MeetingPresences_All` | `IList<MeetingPresenceEntity>` | 5 min | MeetingPresence create/update/delete |
| `MeetingPresence_{id}` | `MeetingPresenceEntity` | 5 min | MeetingPresence update/delete |
| `Member_{uuid}` | `MemberEntity` | 5 min | Member create/update/delete |
| `CarnivalBlockMembers_Member_{memberId}` | `IList<CarnivalBlockMembersEntity>` | 5 min | Membership create/update/delete |

Cascading soft deletes must also invalidate the caches of the rows they touch.

---

## Containerization

The API ships as a Docker image built from a multi-stage `Dockerfile` at the repo root, and is deployed on Dokploy (self-hosted) alongside a self-hosted Postgres container.

- **Multi-stage build**: `mcr.microsoft.com/dotnet/sdk:8.0` build stage (per-csproj `COPY` for layer-cached restore, then `dotnet publish ./BlocoNaRua.Restful/BlocoNaRua.Restful.csproj -c Release -o /app/publish`) → `mcr.microsoft.com/dotnet/aspnet:8.0` runtime stage.
- **Runtime**: `EXPOSE 8080`; `ENV ASPNETCORE_URLS=http://0.0.0.0:8080`; `ENTRYPOINT ["dotnet", "BlocoNaRua.Restful.dll"]`.
- **`.dockerignore`** excludes `bin/`, `obj/`, `.git/`, `.tmp/`, `.opencode/`, `.github/`, `BlocoNaRua.Tests/`, `*.md`.
- `appsettings.Development.json` never reaches publish output (csproj sets `CopyToPublishDirectory="Never"`), so secrets/config come from environment variables only.
- Deployment target is **Dokploy** (self-hosted); Postgres runs via Dokploy docker-compose.

---

## Naming Conventions (.NET)

| Type | Convention | Example |
|------|------------|---------|
| Files | PascalCase | `CarnivalBlocksRepository.cs` |
| Classes | PascalCase | `CarnivalBlockService` |
| Interfaces | I prefix | `ICarnivalBlockService` |
| Entities | PascalCase + Entity | `CarnivalBlockEntity` |
| DTOs | PascalCase + Action/Response | `CarnivalBlockCreate`, `CarnivalBlockResponse` |
| Mappers | `<Entity>Mapper` static | `CarnivalBlockMapper` |
| Fields | `_camelCase` | `_repository`, `_cache` |
| Properties | PascalCase | `CarnivalBlockImage` |
| Methods | PascalCase, `Async` suffix | `GetAllAsync` |
| Route | `api/v1/<ControllerName>` | `api/v1/CarnivalBlocks` |
| DB columns | `snake_case` via `HasColumnName` | `invite_code` |
| Roles enum values | PascalCase | `RolesEnum.Owner` |

Full reference: `lookup/naming.md`.

---

## Configuration

```jsonc
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=5432;Database=...;Username=...;Password=..."
  },
  "Auth": {
    "SigningKey": "<env-only, base64 or raw UTF-8, >= 32 bytes>",
    "Issuer": "bloconarua-api",
    "Audience": "bloconarua-api",
    "AdminEmails": "<comma-separated emails granted role=admin>"
  },
  "Authentication": {
    "Google": {
      "ClientId": "<from Google Cloud Console>",
      "ClientSecret": "<backend-only secret>"
    }
  },
  "Uploads": {
    "BasePath": "/app/uploads",
    "MaxSizeBytes": 5242880,
    "AllowedExtensions": ".jpg,.jpeg,.png,.webp"
  }
}
```
- `BlocoNaRua.Data/Extensions/DataExtensions.AddEntityFramework` resolves the connection string from `appsettings` first, then the `POSTGRESQLCONNSTR_SupabaseDB` env var.
- `Auth:SigningKey` drives HS256 token issuing/validation (self-issued; issuer/audience default `bloconarua-api`). It is env-only — never commit it or return it to clients; rotating it invalidates every issued session. The `Supabase__*` keys are gone (section removed from `appsettings.Development.json`).
- Swagger JSON is served in every environment (`UseSwagger`). Swagger UI and the `/` redirect are mounted in Development only.

---

## Error Pipeline

`Program.cs` (`UseExceptionHandler`, lines 137-181) handles unhandled exceptions:

1. Builds an `ErrorLogEntry` (`BlocoNaRua.Services/Interfaces/IErrorLogger.cs`) from the exception and request:
   - `Level`: `Critical` for `OutOfMemoryException`, `StackOverflowException`, `AccessViolationException`; otherwise `Error`.
   - `Source`: `"ExceptionHandler"`; `Message` and `StackTrace` come from the inner exception when present.
   - `RequestPath`, `RequestMethod`, `StatusCode`, and `UserId` (from the literal `sub` claim — `MapInboundClaims = false` keeps claim names as issued).
2. Calls `IErrorLogger.LogAsync(entry)` (fire-and-forget, see caveat below).
3. Writes a JSON 500 body to the client:
   ```json
   { "StatusCode": 500, "DateTime": "...", "Message": "Internal Server Error.", "Detailed": "<ex.Message>" }
   ```

- Controllers translate known exceptions to 4xx before the global handler fires.
- Caveat: the call is `_ = errorLogger.LogAsync(logEntry);`. The task is not awaited, and a failed write is not observed.

---

## Error Logging

- **Interface** — `IErrorLogger` (`BlocoNaRua.Services/Interfaces/IErrorLogger.cs`) exposes `LogAsync(ErrorLogEntry)` plus `LogWarningAsync`, `LogErrorAsync`, and `LogCriticalAsync` helpers.
- **Implementation** — `ApiErrorLoggerService` (`BlocoNaRua.Services/Implementations/ApiErrorLoggerService.cs`). It writes error rows straight to the `api_errors` table through EF Core (`AppDbContext.ApiErrors`). Same `IErrorLogger` contract as before; a failed insert is caught and logged to the console — logging never throws.
- **Registration** — scoped in `BlocoNaRua.Services/Extensions/ServicesExtensions.cs`.
- **Request logging** — `RequestLoggingMiddleware` (`BlocoNaRua.Restful/Middleware/RequestLoggingMiddleware.cs`, registered at `Program.cs:109`) writes one `ILogger` line per request: method, path, status, duration, and content headers. The `Authorization` header is logged as `Bearer [token]`, never the token itself. The middleware does not write to the database; only the exception handler and error logger do.

Decision record: `lookup/decisions.md`.

---

## 📂 Codebase References

- **Solution**: `BlocoNaRua.sln`
- **Core abstractions**: `BlocoNaRua.Core/Models/EntityBase.cs`, `BlocoNaRua.Core/Models/IRepositoryBase.cs`
- **Domain entities**: `BlocoNaRua.Domain/Entities/`
- **Enum**: `BlocoNaRua.Domain/Enums/RolesEnum.cs`
- **DbContext + mappings**: `BlocoNaRua.Data/Context/AppDbContext.cs`, `BlocoNaRua.Data/Mapping/` (global `HasQueryFilter` for soft delete)
- **Repositories**: `BlocoNaRua.Data/Repositories/` (base: `Repositories/Base/RepositoryBase.cs`)
- **Services**: `BlocoNaRua.Services/Implementations/`
- **Service interfaces**: `BlocoNaRua.Services/Interfaces/` (incl. `IMemberIdentityService.cs`, `IErrorLogger.cs`)
- **Controllers**: `BlocoNaRua.Restful/Controllers/` (incl. `AuthController.cs`, `AdminController.cs`)
- **Middleware**: `BlocoNaRua.Restful/Middleware/RequestLoggingMiddleware.cs`
- **Controller extensions**: `BlocoNaRua.Restful/Extensions/ControllerBaseExtensions.cs`
- **DTOs**: `BlocoNaRua.Restful/Models/<Entity>/`
- **Mappers**: `BlocoNaRua.Restful/Mappers/`
- **Tests**: `BlocoNaRua.Tests/{Restful,Services,Helpers,Integration,Infrastructure}/`
- **Resolved backend tickets**: `BACKEND_TICKETS.md` (repo root, deleted 2026-11-17 — resolutions live in `lookup/living-notes.md` Archive)

---

## Related Files

- `business-domain.md` — Why this exists
- `business-tech-bridge.md` — Feature → implementation mapping
- `identity-auth.md` — Login, JWT validation, identity resolution, admin policy (full flow)
- `guides/api-patterns.md` — Consumer-facing API conventions
- `examples/component-patterns.md` — Copy-paste code patterns, including the integration-test pattern
- `lookup/decisions.md` — Why each architectural choice was made
- `lookup/living-notes.md` — Active issues and debt
- `lookup/naming.md` — Naming reference
