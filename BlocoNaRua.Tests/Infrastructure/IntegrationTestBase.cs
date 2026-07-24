using System.Security.Claims;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BlocoNaRua.Tests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private BlocoNaRuaWebApplicationFactory? _factory;
    private IServiceScope? _scope;

    public virtual async Task InitializeAsync()
    {
        _factory = new BlocoNaRuaWebApplicationFactory();
        Client = _factory.CreateClient();

        // Seed a default member so tests have a valid authenticated identity
        var defaultUuid = Guid.NewGuid();
        var (memberId, memberUuid) = await SeedMember("Default Member", "default@test.com", defaultUuid);
        CurrentMemberId = memberId;
        CurrentMemberUuid = memberUuid;
    }

    public Task DisposeAsync()
    {
        _scope?.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    protected HttpClient Client { get; private set; } = null!;

    protected int CurrentMemberId { get; private set; }

    protected Guid CurrentMemberUuid { get; private set; }

    protected async Task<(int memberId, Guid uuid)> SeedMember(string name, string email, Guid uuid)
    {
        var entity = new MemberEntity(0, name, email, string.Empty, string.Empty, uuid);

        var scope = _factory!.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMembersRepository>();
        var created = await repository.AddAsync(entity);

        SetCurrentMember(uuid);

        return (created.Id, uuid);
    }

    protected void SetCurrentMember(Guid uuid)
    {
        _factory!.SetCurrentMember(uuid);
    }

    protected async Task<int> SeedCarnivalBlockMember(int carnivalBlockId, int memberId, RolesEnum role)
    {
        var scope = _factory!.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICarnivalBlockMembersRepository>();
        var entity = new CarnivalBlockMembersEntity(0, carnivalBlockId, memberId, role);
        var created = await repository.AddAsync(entity);
        return created.Id;
    }

    protected AppDbContext GetDbContext()
    {
        _scope?.Dispose();
        _scope = _factory!.Services.CreateScope();
        return _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
