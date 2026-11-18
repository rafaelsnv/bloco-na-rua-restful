<!-- Context: project-intelligence/naming | Priority: high | Version: 2.1 | Updated: 2026-11-17 -->

# Naming Conventions

> Canonical naming for the BlocoNaRua .NET 8 backend. Use this when generating or reviewing code.

## File naming

| Type | Convention | Example |
|------|------------|---------|
| All C# files | PascalCase, matching primary type | `CarnivalBlocksRepository.cs` |
| Test files | `<TypeUnderTest>Tests.cs` | `CarnivalBlockServiceTests.cs` |
| Test helper | descriptive PascalCase | `TestDbContextFactory.cs` |
| Config files | `appsettings.{Environment}.json` | `appsettings.Development.json` |

## Namespaces

```
BlocoNaRua.<Project>           # root
BlocoNaRua.<Project>.<Folder>  # deep
```
Examples:
- `BlocoNaRua.Domain.Entities`
- `BlocoNaRua.Data.Repositories`
- `BlocoNaRua.Services.Implementations`
- `BlocoNaRua.Restful.Controllers`

## Class naming

| Type | Convention | Example |
|------|------------|---------|
| Concrete class | PascalCase, noun | `CarnivalBlockService` |
| Abstract / base | PascalCase, `Base` suffix optional | `EntityBase`, `RepositoryBase<T>` |
| Interface | PascalCase + `I` prefix | `ICarnivalBlockService`, `IRepositoryBase<T>` |
| Entity | PascalCase + `Entity` suffix | `CarnivalBlockEntity` |
| Repository | PascalCase + `Repository` suffix | `CarnivalBlocksRepository` |
| Service | PascalCase + `Service` suffix | `CarnivalBlockService` |
| Controller | PascalCase + `Controller` suffix | `CarnivalBlocksController` |
| DTO | PascalCase + action / `Response` suffix | `CarnivalBlockCreate`, `MeetingResponse` |
| Mapper | `<Entity>Mapper` static class | `CarnivalBlockMapper` |
| Exception class | PascalCase + `Exception` suffix | (use built-ins: `KeyNotFoundException`, `UnauthorizedAccessException`) |
| Enum | PascalCase, singular | `RolesEnum` |
| Extension class | PascalCase + `Extensions` suffix | `DataExtensions`, `ServicesExtensions` |

## Member naming

| Type | Convention | Example |
|------|------------|---------|
| Public property | PascalCase | `CarnivalBlockImage`, `InviteCode`, `IsActive` |
| Private field | `_camelCase` | `_repository`, `_cache`, `_memberIdentityService` |
| Constant | PascalCase (or `k`-prefixed if you must) | `DefaultConnection` |
| Method (sync) | PascalCase, verb | `GetAll`, `GetMemberId` |
| Method (async) | PascalCase, `Async` suffix | `GetByIdAsync`, `GetMemberIdAsync`, `CreateAsync` |
| Inline role helper | `GetMemberRoleInline` (private, returns `Task<RolesEnum?>`) | `GetMemberRoleInline(carnivalBlockId, memberId)` |
| Soft-delete flag | `IsActive` (`bool`, default `true`) | `entity.IsActive = true` |
| Local variable | camelCase | `carnivalBlock`, `loggedMemberId` |
| Parameter | camelCase | `loggedMember`, `carnivalBlockId` |
| Constructor parameter (primary) | camelCase | `(ICarnivalBlocksRepository repository, IMemoryCache cache)` |
| Enum value | PascalCase | `RolesEnum.Owner`, `RolesEnum.Manager` |

## Code examples

```csharp
// ✅ Correct
public class CarnivalBlockService(
    ICarnivalBlocksRepository repository,
    IMembersRepository membersRepository,
    IMemoryCache cache) : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _repository = repository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly IMemoryCache _cache = cache;

    private async Task<RolesEnum?> GetMemberRoleInline(int carnivalBlockId, int memberId) { /* ... */ }

    public async Task<CarnivalBlockEntity?> GetByIdAsync(int id) { /* ... */ }
    public async Task<CarnivalBlockEntity> CreateAsync(CarnivalBlockEntity model) { /* ... */ }
}

// Controller identity resolution via IMemberIdentityService (_memberIdentityService)
var memberId = await _memberIdentityService.GetMemberIdAsync();
// or via ControllerBaseExtensions: this.GetMemberId(_memberIdentityService)

// ❌ Incorrect
public class carnivalBlockService { }                  // wrong case
public class CarnivalBlockSvc { }                      // abbreviation
private ICarnivalBlocksRepository repository;          // missing _ prefix
public async Task<CarnivalBlockEntity> GetById(int id) // missing Async suffix on async method
```

## API surface naming

| Aspect | Convention | Example |
|--------|------------|---------|
| Route prefix | `api/v{version:apiVersion}` | `api/v1/CarnivalBlocks` |
| Controller name | Singular, matches entity | `CarnivalBlocksController` |
| Auth header | Standard Bearer scheme | `Authorization: Bearer <token>` |
| Query parameter | camelCase | `?page=2&pageSize=20` |
| JSON property | PascalCase (default) | `"InviteCode": "A8K2QZ9X"` |
| HTTP method | Verb (GET/POST/PUT/DELETE) | — |
| Response status | Standard codes | `200`, `201`, `204`, `400`, `401`, `404`, `500` |

## Database naming

| Aspect | Convention | Example |
|--------|------------|---------|
| Table | snake_case, plural | `carnival_blocks`, `carnival_block_members` |
| Column | snake_case | `invite_code`, `managers_invite_code`, `meeting_date_time`, `is_active` |
| Primary key | `id` | `id` |
| FK column | `<table>_id` | `carnival_block_id`, `member_id` |
| Enum (DB side) | Postgres enum, PascalCase values | `RolesEnum.Member`, `RolesEnum.Manager`, `RolesEnum.Owner` |
| Constraint | `<table>_<col>_fkey` | `carnival_blocks_owner_id_fkey` |
| Index | created via `HasIndex(...)` when needed | `HasIndex(u => u.Uuid).IsUnique()` |

## Cache keys

| Pattern | Example |
|---------|---------|
| Whole collection | `<Entity>_All` (`CarnivalBlocks_All`, `Meetings_All`) |
| Single entity | `<Entity>_{id}` (`Meeting_42`, `Member_{uuid}`) |
| Member-scoped | `CarnivalBlockMembers_Member_{memberId}` |
| Uuid-scoped | `Member_{uuid}` |

## Test naming

| Method | Convention |
|--------|------------|
| Test method | `<MethodName>_Should<ExpectedBehavior>_When<Condition>` |
| Arrange / Act / Assert | Use comment dividers |
| Test class | `<TypeUnderTest>Tests` |
| Test category | xUnit `[Fact]` for unit, `[Theory]` for parameterized |

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnEntity_WhenBlockExists() { /* ... */ }

[Fact]
public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenOwnerDoesNotExist() { /* ... */ }
```

## 📂 Codebase References

- **Entities**: `BlocoNaRua.Domain/Entities/`
- **Enums**: `BlocoNaRua.Domain/Enums/RolesEnum.cs`
- **Repository interfaces**: `BlocoNaRua.Data/Repositories/Interfaces/`
- **Service interfaces**: `BlocoNaRua.Services/Interfaces/`
- **DTOs**: `BlocoNaRua.Restful/Models/<Entity>/`
- **Mappers**: `BlocoNaRua.Restful/Mappers/`

## Related Files

- `../concepts/technical-domain.md` — Stack and architecture
- `../guides/api-patterns.md` — API endpoint conventions
- `../examples/component-patterns.md` — Component patterns