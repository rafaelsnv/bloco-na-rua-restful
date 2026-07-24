using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlocoNaRua.Restful.Models.Member;
using BlocoNaRua.Tests.Infrastructure;

namespace BlocoNaRua.Tests.Integration;

public class MembersApiIntegrationTests : IntegrationTestBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/members");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var members = await response.Content.ReadFromJsonAsync<List<MemberResponse>>(_jsonOptions);
        Assert.NotNull(members);
        Assert.NotEmpty(members);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/members/{CurrentMemberId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberResponse>(_jsonOptions);
        Assert.NotNull(member);
        Assert.Equal(CurrentMemberId, member.Id);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/members/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created()
    {
        // Arrange
        var newUuid = Guid.NewGuid().ToString();
        var createDto = new MemberCreate(
            Name: "New Member",
            Email: "newmember@example.com",
            Phone: "+5511999999999",
            ProfileImage: "new.jpg",
            Uuid: newUuid
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/members", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("Location"));

        var created = await response.Content.ReadFromJsonAsync<MemberResponse>(_jsonOptions);
        Assert.NotNull(created);
        Assert.Equal("New Member", created.Name);
        Assert.Equal("newmember@example.com", created.Email);
    }

    [Fact]
    public async Task Update_Returns200_WhenOwner()
    {
        // Arrange
        var updateDto = new MemberUpdate(
            Name: "Updated Name",
            Email: "updated@example.com",
            Phone: "+5511888888888",
            ProfileImage: "updated.jpg"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/members/{CurrentMemberId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<MemberResponse>(_jsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("updated@example.com", updated.Email);
    }

    [Fact]
    public async Task Update_Returns401_WhenNotOwner()
    {
        // Arrange - seed a second member (sets them as current authenticated user)
        await SeedMember("Second Member", "second@example.com", Guid.NewGuid());

        var updateDto = new MemberUpdate(
            Name: "Attempted Update",
            Email: "hacked@example.com",
            Phone: "+5511777777777",
            ProfileImage: "hacked.jpg"
        );

        // Act - try to update the first member as the second member
        var response = await Client.PutAsJsonAsync($"/api/v1/members/{CurrentMemberId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204_WhenOwner()
    {
        // Arrange - create a member to delete
        var deleteUuid = Guid.NewGuid().ToString();
        var createDto = new MemberCreate(
            Name: "To Delete",
            Email: "delete@example.com",
            Phone: "+5511666666666",
            ProfileImage: "delete.jpg",
            Uuid: deleteUuid
        );
        var createResponse = await Client.PostAsJsonAsync("/api/v1/members", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<MemberResponse>(_jsonOptions);
        Assert.NotNull(created);
        var toDeleteId = created.Id;

        // Set current member to the owner of the created member
        SetCurrentMember(new Guid(deleteUuid));

        // Act
        var response = await Client.DeleteAsync($"/api/v1/members/{toDeleteId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns401_WhenNotOwner()
    {
        // Arrange - seed a second member (sets them as current authenticated user)
        await SeedMember("Second Deleter", "deleter@example.com", Guid.NewGuid());

        // Act - try to delete the first member as the second member
        var response = await Client.DeleteAsync($"/api/v1/members/{CurrentMemberId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByUuid_ReturnsOk()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/members/uuid/{CurrentMemberUuid}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberResponse>(_jsonOptions);
        Assert.NotNull(member);
        Assert.Equal(CurrentMemberId, member.Id);
    }
}
