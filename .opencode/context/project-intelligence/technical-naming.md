<!-- Context: project-intelligence/technical-naming | Priority: high | Version: 1.0 | Updated: 2026-05-17 -->

# Naming Conventions

**Purpose**: Complete naming conventions for BlocoNaRua .NET API.
**Audience**: AI agents generating .NET code

## File Naming

| Type | Convention | Example |
|------|------------|---------|
| All code files | PascalCase | `CarnivalBlocksRepository.cs` |
| Test files | `*Tests.cs` | `CarnivalBlockServiceTests.cs` |

## Class Naming

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `CarnivalBlockService` |
| Entities | PascalCase + Entity | `CarnivalBlockEntity` |
| Interfaces | I prefix | `ICarnivalBlockService` |
| Enums | PascalCase + Enum | `RolesEnum` |
| DTOs (records) | PascalCase + suffix | `CarnivalBlockCreate`, `CarnivalBlockResponse` |
| Mappers | PascalCase + Mapper | `CarnivalBlockMapper` |
| Controllers | PascalCase + Controller | `CarnivalBlocksController` |
| Configuration | PascalCase + Configuration | `CarnivalBlockConfiguration` |

## Member Naming

| Type | Convention | Example |
|------|------------|---------|
| Instance fields | _camelCase | `_repository` |
| Static fields | s_camelCase | `s_defaultValue` |
| Local variables | camelCase | `carnivalBlock` |
| Parameters | camelCase | `entityId` |
| Properties | PascalCase | `CarnivalBlockImage` |
| Methods | PascalCase | `GetAllAsync` |

## Enum Naming

```csharp
public enum RolesEnum { Member, Manager, Owner }
```

## Route Naming

| Type | Convention | Example |
|------|------------|---------|
| URL paths | kebab-case | `/carnival-blocks` |
| Route params | camelCase | `:id` |
| Header params | PascalCase | `X-Logged-Member` |

## Code Examples

```csharp
// ✅ Correct
public class CarnivalBlockService { }
public interface ICarnivalBlockService { }
public class CarnivalBlockEntity { }
public record CarnivalBlockCreate(string Name);
private readonly ICarnivalBlockService _service;
var carnivalBlocks = await _service.GetAllAsync();

// ❌ Incorrect
public class carnival_block_service { }  // Should be PascalCase
const MAX_SIZE = 100;                     // Should be PascalCase
```

## 📂 Codebase References

**Entities**: `BlocoNaRua.Domain/Entities/`
**Services**: `BlocoNaRua.Services/Implementations/`
**Repositories**: `BlocoNaRua.Data/Repositories/`
**Controllers**: `BlocoNaRua.Restful/Controllers/`

## Related Files

- `technical-domain.md` - Entry point
- `technical-component-pattern.md` - Service patterns
- `technical-api-pattern.md` - API patterns