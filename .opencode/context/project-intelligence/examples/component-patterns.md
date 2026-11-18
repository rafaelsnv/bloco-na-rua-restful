<!-- Context: project-intelligence/technical-component | Priority: high | Version: 2.1 | Updated: 2026-11-17 -->

# Component Patterns

> Patterns for the .NET 8 layered backend: entity, repository, service, controller, mapper, DTO.

## Entity pattern

```csharp
public class XxxEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public Guid Uuid { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public ICollection<YyyEntity> Yyys { get; set; } = [];

    public XxxEntity(int id, string name, Guid uuid) : this(id)
    {
        Name = name;
        Uuid = uuid;
    }
}
```

- `: EntityBase(id)` — `Id` becomes a private setter; mutations flow through services.
- Default-initialized strings and collections satisfy nullable reference types.
- `IsActive` drives soft-delete; the global query filter hides inactive rows.
- A second constructor chains `: this(id)` then assigns properties — useful for repositories / tests.

## Repository pattern

```csharp
public interface IXxxRepository : IRepositoryBase<XxxEntity>
{
    Task<IList<XxxEntity>> GetBySomethingAsync(int id, CancellationToken ct);
}

public class XxxRepository(AppDbContext appContext) : RepositoryBase<XxxEntity>(appContext), IXxxRepository
{
    public async Task<IList<XxxEntity>> GetBySomethingAsync(int id, CancellationToken ct) =>
        await DbSet.AsNoTracking().Where(e => e.SomethingId == id).ToListAsync(ct);
}
```

- Inherit from `RepositoryBase<TEntity>` for free `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
- Custom queries: prefer `AsNoTracking()` for read paths; let `AddAsync`/`UpdateAsync` track for write.
- Repository methods take a `CancellationToken` (`ct`) and pass it to EF Core (`ToListAsync(ct)`, `FindAsync([id], ct)`).
- Deletes are **soft**: set `IsActive = false` (the query filter hides inactive rows).
- All repositories are registered in `BlocoNaRua.Data/Extensions/DataExtensions.AddRepositories()`.

## Service pattern

```csharp
public class XxxService
(
    IXxxRepository repository,
    IXxxMembersRepository xxxMembersRepository,
    IMemoryCache cache
) : IXxxService
{
    private readonly IXxxRepository _repository = repository;
    private readonly IXxxMembersRepository _xxxMembersRepository = xxxMembersRepository;
    private readonly IMemoryCache _cache = cache;

    // Authorization is inline: there is no shared authorization service.
    private async Task<RolesEnum?> GetMemberRoleInline(int xxxId, int memberId)
    {
        var entity = await _repository.GetByIdAsync(xxxId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Xxx does not exist.");
        if (entity.OwnerId == memberId)
            return RolesEnum.Owner;
        return await _xxxMembersRepository.GetMemberRole(xxxId, memberId, CancellationToken.None);
    }

    public async Task<XxxEntity?> GetByIdAsync(int id)
    {
        if (_cache.TryGetValue($"Xxx_{id}", out XxxEntity? hit))
            return hit;

        var fresh = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (fresh != null)
            _cache.Set($"Xxx_{id}", fresh, TimeSpan.FromMinutes(5));
        return fresh;
    }

    public async Task<XxxEntity?> UpdateAsync(int id, int loggedMember, XxxEntity model)
    {
        var entity = await _repository.GetByIdAsync(id, CancellationToken.None)
            ?? throw new KeyNotFoundException("Xxx does not exist.");

        var memberRole = await GetMemberRoleInline(id, loggedMember);
        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
            throw new UnauthorizedAccessException("Member is not authorized to update this xxx.");

        entity.Name = model.Name;
        await _repository.UpdateAsync(id, entity, CancellationToken.None);
        _cache.Remove($"Xxx_{id}");
        _cache.Remove("Xxxs_All");
        return entity;
    }
}
```

Rules:
- Primary constructor for DI, then `private readonly _field = field;` for readability inside method bodies.
- Reads cache first, fall through to repo, populate cache on hit.
- Writes always invalidate the relevant cache keys (see `technical-domain.md` table).
- **Authorization is inline per service** — a private `GetMemberRoleInline(...)` helper reads the member's role and throws `UnauthorizedAccessException` when the role is insufficient. The shared authorization abstraction was removed (commit `0581d26`); do not reintroduce it.
- Repository calls take a `CancellationToken` (services currently pass `CancellationToken.None`).
- Authorization happens here, not in the controller — the controller only resolves the caller id and passes it down.

## Controller pattern

```csharp
[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(List<XxxResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class XxxController(IXxxService service, IMemberIdentityService memberIdentityService) : ControllerBase
{
    private readonly IXxxService _service = service;
    private readonly IMemberIdentityService _memberIdentityService = memberIdentityService;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(XxxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();
        return Ok(entity.ToDTO());
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(XxxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] XxxUpdate model)
    {
        // memberId comes from the JWT "sub" claim — no logged-member header.
        var memberId = this.GetMemberId(_memberIdentityService);
        try
        {
            var updated = await _service.UpdateAsync(id, memberId, /* entity */);
            if (updated is null) return NotFound();
            return Ok(updated.ToDTO());
        }
        catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException)  { return Unauthorized(/* message */); }
        catch (InvalidOperationException)    { return BadRequest(/* message */); }
    }
}
```

- `[ApiController]` enables automatic 400 on model validation errors.
- Always declare `[ProducesResponseType]` for OpenAPI / Swagger.
- Controllers never instantiate repositories directly — only through services.
- **The caller id comes from the JWT, not a header.** `ControllerBaseExtensions.GetMemberId(this ControllerBase, IMemberIdentityService)` (`BlocoNaRua.Restful/Extensions/ControllerBaseExtensions.cs`, `using BlocoNaRua.Restful.Extensions;`) reads the `sub` claim, delegates to `IMemberIdentityService.GetMemberIdAsync()` and throws `UnauthorizedAccessException` when the claim is missing. Inject `IMemberIdentityService` in the primary constructor and call `this.GetMemberId(...)`. Controllers that prefer `async` may call `await _memberIdentityService.GetMemberIdAsync()` directly — both resolve the same `sub` claim.

## DTO pattern (record)

```csharp
namespace BlocoNaRua.Restful.Models.Xxx;

public record XxxCreate(string Name, string Description);
public record XxxUpdate(string Name);
public record XxxResponse(int Id, string Name, string Description, DateTime? CreatedAt, DateTime? UpdatedAt);
```

- One folder per entity: `BlocoNaRua.Restful/Models/<Entity>/`.
- Parameter DTOs (POST/PUT body) use `record class` (e.g. `CarnivalBlockCreate`, `CarnivalBlockUpdate`); response DTOs use plain `record`.
- Use validation attributes: `[EmailAddress]`, `[Phone]`, `[RegularExpression]` (see `MemberCreate`).

## Mapper pattern (static)

```csharp
public static class EntityMapper
{
    public static XxxResponse ToDTO(this XxxEntity entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.CreatedAt, entity.UpdatedAt);
}
```

- One static class per layer — `BlocoNaRua.Restful/Mappers/EntityMapper.cs` holds every `ToDTO` extension method.
- Located in `BlocoNaRua.Restful/Mappers/`.
- Mappers never touch the database — pure functions from entity to DTO.

## Test pattern

```csharp
public class XxxServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly XxxRepository _xRepository;
    private readonly XxxMembersRepository _xxxMembersRepository;
    private readonly IMemoryCache _cache;
    private readonly XxxService _service;

    public XxxServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        _context = TestDbContextFactory.GetContext(dbName);
        _xRepository = new XxxRepository(_context);
        _xxxMembersRepository = new XxxMembersRepository(_context);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new XxxService(_xRepository, _xxxMembersRepository, _cache);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists() { /* Arrange/Act/Assert */ }
}
```

```csharp
public class XxxControllerTests
{
    private readonly Mock<IXxxService> _serviceMock = new();
    private readonly Mock<IMemberIdentityService> _memberIdentityServiceMock = new();

    private XxxController CreateController() => new(_serviceMock.Object, _memberIdentityServiceMock.Object);

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenEntityMissing()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((XxxEntity?)null);
        var result = await CreateController().GetById(1);
        Assert.IsType<NotFoundResult>(result);
    }
}
```

- **Service tests**: real `DbContext` (InMemory) + real repositories, real `IMemoryCache`. There is no authorization mock — authorization is inline in the service, so the test asserts on `UnauthorizedAccessException` thrown by the service itself. `IDisposable` + `EnsureDeleted` for isolation.
- **Controller tests**: mocked service + mocked `IMemberIdentityService` (stub `GetMemberIdAsync()` to return the acting member id); only HTTP-shape assertions.

## Integration Test Pattern

```csharp
// BlocoNaRuaWebApplicationFactory<TProgram> — lives in BlocoNaRua.Tests/Helpers/
public class BlocoNaRuaWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // triggers InMemory swap in DataExtensions
    }
}
```

```csharp
// TestAuthHandler.cs — lives in BlocoNaRua.Tests/Helpers/
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly Guid _memberUuid;
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _memberUuid.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), "TestScheme")));
    }
}
```

```csharp
// IntegrationTestBase.cs — lives in BlocoNaRua.Tests/Infrastructure/
public class IntegrationTestBase<TWebApp> : IAsyncLifetime where TWebApp : class
{
    private readonly BlocoNaRuaWebApplicationFactory<TWebApp> _factory = new();
    public HttpClient Client => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        // Seed test member into InMemory DB via scoped service scope
    }

    public async Task DisposeAsync() => _factory.Dispose();
}
```

### Key Patterns
- **Fresh factory per test** via `IAsyncLifetime` — ensures cache/state isolation
- **Logged-member header parameter** removed — the member id now comes from the JWT `sub` claim
- **InMemory ≠ Postgres**: FK constraints not enforced; document gaps with `// ponytail:`
- **Auth bypass**: `TestAuthHandler` short-circuits before JWT validation; `TestCurrentMemberHolder` carries UUID via `AsyncLocal`

### Files Created
- `BlocoNaRua.Tests/Helpers/TestAuthHandler.cs`
- `BlocoNaRua.Tests/Helpers/BlocoNaRuaWebApplicationFactory.cs`
- `BlocoNaRua.Tests/Infrastructure/IntegrationTestBase.cs`
- `BlocoNaRua.Tests/Integration/{Members,CarnivalBlocks,Meetings,MeetingPresence}ApiIntegrationTests.cs`

### Reference
- `BlocoNaRua.Tests/Services/MembersServiceTests.cs` — service-level unit tests (InMemory repo)
- `BlocoNaRua.Tests/Restful/MembersControllerTests.cs` — controller unit tests (mocked service)

---

## 📂 Codebase References

- **Entities**: `BlocoNaRua.Domain/Entities/`
- **Base abstractions**: `BlocoNaRua.Core/Models/`
- **Repositories**: `BlocoNaRua.Data/Repositories/`
- **Services**: `BlocoNaRua.Services/Implementations/`
- **Controllers**: `BlocoNaRua.Restful/Controllers/`
- **DTOs**: `BlocoNaRua.Restful/Models/<Entity>/`
- **Mappers**: `BlocoNaRua.Restful/Mappers/`
- **Tests**: `BlocoNaRua.Tests/{Restful,Services,Helpers,Infrastructure,Integration,Controllers}/`

## Related Files

- `../concepts/technical-domain.md` — Stack and architecture overview
- `../concepts/identity-auth.md` — JWT auth, member identity, admin policy
- `../guides/api-patterns.md` — API endpoint conventions
- `../lookup/naming.md` — Naming reference
