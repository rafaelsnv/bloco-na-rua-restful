<!-- Context: project-intelligence/technical-api | Priority: high | Version: 1.0 | Updated: 2026-05-17 -->

# API Patterns

**Purpose**: Detailed API patterns for BlocoNaRua REST API.
**Audience**: AI agents implementing API endpoints

## Controller Pattern

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CarnivalBlocksController(ICarnivalBlockService service) : ControllerBase
{
    private readonly ICarnivalBlockService _service = service;
}
```

## Route Conventions

| Pattern | Example |
|---------|---------|
| Route template | `api/v{version:apiVersion}/[controller]` |
| Version format | `'v'VVV` (e.g., `v1`) |
| Controller → URL | `CarnivalBlocksController` → `/carnival-blocks` |

## HTTP Methods

| Method | Usage | Return |
|--------|-------|--------|
| `[HttpGet]` | List all / Get by ID | `Ok(list)` or `NotFound()` |
| `[HttpPost]` | Create | `CreatedAtAction(nameof(GetById), new { id }, result)` |
| `[HttpPut("{id}")]` | Update with auth | `Ok(result)` / `NotFound()` / `Unauthorized()` |
| `[HttpDelete("{id}")]` | Delete with auth | `NoContent()` / `NotFound()` / `Unauthorized()` |

## DTO Pattern (Records)

```csharp
// Create - required fields only
public record class CarnivalBlockCreate(string Name, int OwnerId, string CarnivalBlockImage);

// Update - optional fields
public record class CarnivalBlockUpdate(string Name, string CarnivalBlockImage);

// Response - all fields
public record CarnivalBlockResponse(int Id, int OwnerId, string Name, string InviteCode, string ManagersInviteCode, string CarnivalBlockImage, DateTime? CreatedAt, DateTime? UpdatedAt);
```

## Error Handling

```csharp
// Service throws exceptions
throw new KeyNotFoundException("Entity does not exist.");     // → 404
throw new UnauthorizedAccessException("Not authorized.");    // → 401

// Controller catches
catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
```

## Response Patterns

| Operation | Response |
|-----------|----------|
| GetAll | `Ok(list.Select(Mapper.ToDTO).ToList())` |
| GetById | `Ok(Mapper.ToDTO(entity))` or `NotFound()` |
| Create | `CreatedAtAction(nameof(GetById), new { id = result.Id }, result)` |
| Update | `Ok(Mapper.ToDTO(updated))` or `NotFound()`/`Unauthorized()` |
| Delete | `NoContent()` or `NotFound()`/`Unauthorized()` |

## Mapper Pattern

```csharp
public static class CarnivalBlockMapper
{
    public static CarnivalBlockResponse ToDTO(CarnivalBlockEntity entity) =>
        new(
            entity.Id,
            entity.OwnerId,
            entity.Name,
            entity.InviteCode,
            entity.ManagersInviteCode,
            entity.CarnivalBlockImage,
            entity.CreatedAt,
            entity.UpdatedAt
        );
}
```

## Header-Based Auth

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(
    int id,
    [FromBody] CarnivalBlockUpdate model,
    [FromHeader(Name = "X-Logged-Member")] int loggedMember)
```

## 📂 Codebase References

**Controllers**: `BlocoNaRua.Restful/Controllers/`
**DTOs**: `BlocoNaRua.Restful/Models/{Entity}/`
**Mappers**: `BlocoNaRua.Restful/Mappers/`

## Related Files

- `technical-domain.md` - Entry point
- `technical-component-pattern.md` - Service patterns
- `technical-naming.md` - Naming conventions