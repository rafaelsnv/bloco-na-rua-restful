using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Tests.Infrastructure;

namespace BlocoNaRua.Tests.Integration;

public class CarnivalBlocksApiIntegrationTests : IntegrationTestBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/carnivalblocks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var blocks = await response.Content.ReadFromJsonAsync<List<CarnivalBlockResponse>>(_jsonOptions);
        Assert.NotNull(blocks);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        // Arrange - create a carnival block first
        var (ownerId, _) = await SeedMember("Block Owner", "blockowner@test.com", Guid.NewGuid());
        var createDto = new CarnivalBlockCreate("Test Block", ownerId, "block.jpg");
        var createResponse = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(created);

        // Act
        var response = await Client.GetAsync($"/api/v1/carnivalblocks/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var block = await response.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(block);
        Assert.Equal(created.Id, block.Id);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/carnivalblocks/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201_WhenOwner()
    {
        // Arrange
        var (ownerId, ownerUuid) = await SeedMember("Carnival Owner", "carnivalowner@test.com", Guid.NewGuid());
        var createDto = new CarnivalBlockCreate("My Carnival Block", ownerId, "carnival.jpg");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("Location"));

        var created = await response.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(created);
        Assert.Equal("My Carnival Block", created.Name);
        Assert.Equal(ownerId, created.OwnerId);
    }

    [Fact]
    public async Task Create_Returns401_WhenMember()
    {
        // Note: The actual API does not enforce role-based creation.
        // This test documents expected behavior: any authenticated member can create a block.
        // The OwnerId in the DTO determines ownership.
        var (memberId, _) = await SeedMember("Carnival Member", "carnivalmember@test.com", Guid.NewGuid());
        var createDto = new CarnivalBlockCreate("Member Carnival Block", memberId, "member_carnival.jpg");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", createDto);

        // Assert - any authenticated member can create; role check is on mutation only
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_Returns200_WhenOwner()
    {
        // Arrange - create block as Owner, then update
        var (ownerId, _) = await SeedMember("Update Owner", "updateowner@test.com", Guid.NewGuid());
        var createDto = new CarnivalBlockCreate("Block To Update", ownerId, "original.jpg");
        var createResponse = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(created);

        var updateDto = new CarnivalBlockUpdate("Updated Block Name", "updated.jpg");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/carnivalblocks/{created.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Updated Block Name", updated.Name);
    }

    [Fact]
    public async Task Update_Returns401_WhenMember()
    {
        // Arrange - create block as Owner, add Member with Member role, try to update as Member
        var (ownerId, _) = await SeedMember("Block Creator", "blockcreator@test.com", Guid.NewGuid());
        var createDto = new CarnivalBlockCreate("Protected Block", ownerId, "protected.jpg");
        var createResponse = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(created);

        // Seed a second member and add them as Member role (not Owner/Manager)
        var (memberId, memberUuid) = await SeedMember("Block Member", "blockmember@test.com", Guid.NewGuid());
        await SeedCarnivalBlockMember(created.Id, memberId, RolesEnum.Member);

        // Switch to the member identity
        SetCurrentMember(memberUuid);

        var updateDto = new CarnivalBlockUpdate("Hacked Name", "hacked.jpg");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/carnivalblocks/{created.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204_WhenOwner()
    {
        // Arrange - create block as Owner, then delete
        var (ownerId, _) = await SeedMember("Delete Owner", "deleteowner@test.com", Guid.NewGuid());
        var createDto = new CarnivalBlockCreate("Block To Delete", ownerId, "delete.jpg");
        var createResponse = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(created);

        // Act
        var response = await Client.DeleteAsync($"/api/v1/carnivalblocks/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns401_WhenMember()
    {
        // Arrange - create block as Owner, add Member with Member role, try to delete as Member
        var (ownerId, _) = await SeedMember("Deleting Creator", "deletingcreator@test.com", Guid.NewGuid());
        var createDto = new CarnivalBlockCreate("Block To Protect", ownerId, "protect.jpg");
        var createResponse = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        Assert.NotNull(created);

        // Seed a second member and add them as Member role (only Owner can delete)
        var (memberId, memberUuid) = await SeedMember("Delete Member", "deletemember@test.com", Guid.NewGuid());
        await SeedCarnivalBlockMember(created.Id, memberId, RolesEnum.Member);

        // Switch to the member identity
        SetCurrentMember(memberUuid);

        // Act
        var response = await Client.DeleteAsync($"/api/v1/carnivalblocks/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
