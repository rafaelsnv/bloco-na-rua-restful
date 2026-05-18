<!-- Context: project-intelligence/technical-component | Priority: high | Version: 1.0 | Updated: 2026-05-17 -->

# Component Patterns

**Purpose**: Detailed service, repository, and DI patterns for BlocoNaRua API.
**Audience**: AI agents implementing business logic and data access

## Service Pattern (Primary Constructor)

```csharp
public class CarnivalBlockService
(
    ICarnivalBlocksRepository repository,
    IMembersRepository membersRepository,
    IAuthorizationService authorizationService
) : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _repository = repository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly IAuthorizationService _authorizationService = authorizationService;
}
```

## Interface Pattern

```csharp
public interface ICarnivalBlockService
{
    Task<IList<CarnivalBlockEntity>> GetAllAsync();
    Task<CarnivalBlockEntity?> GetByIdAsync(int id);
    Task<CarnivalBlockEntity> CreateAsync(CarnivalBlockEntity entity);
    Task<CarnivalBlockEntity?> UpdateAsync(int id, int loggedMember, CarnivalBlockEntity entity);
    Task<bool> DeleteAsync(int id, int loggedMember);
}
```

## Repository Pattern

```csharp
public interface ICarnivalBlocksRepository
{
    Task<IList<CarnivalBlockEntity>> GetAllAsync();
    Task<CarnivalBlockEntity?> GetByIdAsync(int id);
    Task<CarnivalBlockEntity> AddAsync(CarnivalBlockEntity entity);
    Task UpdateAsync(CarnivalBlockEntity entity);
    Task<bool> DeleteAsync(CarnivalBlockEntity entity);
}

public class CarnivalBlocksRepository(AppDbContext appContext)
    : RepositoryBase<CarnivalBlockEntity>(appContext), ICarnivalBlocksRepository;
```

## Authorization Pattern

```csharp
var memberRole = await _authorizationService.GetMemberRole(carnivalBlockId, memberId);
if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
    throw new UnauthorizedAccessException();
```

## Exception Handling

```csharp
// Service throws
throw new KeyNotFoundException("Entity does not exist.");
throw new UnauthorizedAccessException("Not authorized.");

// Controller catches
catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
```

## Entity Pattern

```csharp
// Base class with primary constructor
public abstract class EntityBase(int id)
{
    public int Id { get; private set; } = id;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Entity with primary constructor
public class CarnivalBlockEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    // ... other properties
}
```

## DI Registration

```csharp
// ServicesExtensions.cs
public static IServiceCollection AddServices(this IServiceCollection services)
{
    services.AddScoped<ICarnivalBlockService, CarnivalBlockService>();
    services.AddScoped<IAuthorizationService, AuthorizationService>();
    // ...
}
```

## 📂 Codebase References

**Services**: `BlocoNaRua.Services/Implementations/`
**Interfaces**: `BlocoNaRua.Services/Interfaces/`
**Repositories**: `BlocoNaRua.Data/Repositories/`
**Entities**: `BlocoNaRua.Domain/Entities/`

## Related Files

- `technical-domain.md` - Entry point
- `technical-api-pattern.md` - API patterns
- `technical-naming.md` - Naming conventions