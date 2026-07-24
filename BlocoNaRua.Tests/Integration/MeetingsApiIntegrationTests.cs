using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Models.Meeting;
using BlocoNaRua.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Tests.Integration;

public class MeetingsApiIntegrationTests : IntegrationTestBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private int _ownerMemberId;
    private Guid _ownerUuid;
    private int _memberMemberId;
    private Guid _memberUuid;
    private int _carnivalBlockId;
    private int _meetingId;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        // Seed owner member
        _ownerUuid = Guid.NewGuid();
        (_ownerMemberId, _) = await SeedMember("Owner Member", "owner@test.com", _ownerUuid);

        // Seed regular member
        _memberUuid = Guid.NewGuid();
        (_memberMemberId, _) = await SeedMember("Regular Member", "member@test.com", _memberUuid);

        // Create a CarnivalBlock as the owner
        SetCurrentMember(_ownerUuid);
        var blockCreate = new CarnivalBlockCreate("Test Block", _ownerMemberId, "block.jpg");
        var blockResponse = await Client.PostAsJsonAsync("/api/v1/carnivalblocks", blockCreate);
        var block = await blockResponse.Content.ReadFromJsonAsync<CarnivalBlockResponse>(_jsonOptions);
        _carnivalBlockId = block!.Id;

        // Create a meeting as the owner
        var meetingCreate = new MeetingCreate(
            Name: "Initial Meeting",
            Description: "Test Description",
            Location: "Test Location",
            MeetingDateTime: DateTime.UtcNow.AddDays(7),
            CarnivalBlockId: _carnivalBlockId
        );
        var meetingResponse = await Client.PostAsJsonAsync("/api/v1/meetings", meetingCreate);
        var meeting = await meetingResponse.Content.ReadFromJsonAsync<MeetingResponse>(_jsonOptions);
        _meetingId = meeting!.Id;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/meetings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var meetings = await response.Content.ReadFromJsonAsync<List<MeetingResponse>>(_jsonOptions);
        Assert.NotNull(meetings);
        Assert.NotEmpty(meetings);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/meetings/{_meetingId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var meeting = await response.Content.ReadFromJsonAsync<MeetingResponse>(_jsonOptions);
        Assert.NotNull(meeting);
        Assert.Equal(_meetingId, meeting.Id);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/meetings/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201_WhenOwner()
    {
        // Arrange
        SetCurrentMember(_ownerUuid);
        var createDto = new MeetingCreate(
            Name: "New Meeting",
            Description: "New Description",
            Location: "New Location",
            MeetingDateTime: DateTime.UtcNow.AddDays(14),
            CarnivalBlockId: _carnivalBlockId
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/meetings", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var meeting = await response.Content.ReadFromJsonAsync<MeetingResponse>(_jsonOptions);
        Assert.NotNull(meeting);
        Assert.Equal("New Meeting", meeting.Name);
    }

    [Fact]
    public async Task Create_Returns401_WhenMember()
    {
        // Arrange
        SetCurrentMember(_memberUuid);
        var createDto = new MeetingCreate(
            Name: "Unauthorized Meeting",
            Description: "Should fail",
            Location: "Nowhere",
            MeetingDateTime: DateTime.UtcNow.AddDays(21),
            CarnivalBlockId: _carnivalBlockId
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/meetings", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_Returns200_WhenOwner()
    {
        // Arrange
        SetCurrentMember(_ownerUuid);
        var updateDto = new MeetingUpdate(
            Name: "Updated Meeting",
            Description: "Updated Description",
            Location: "Updated Location",
            MeetingDateTime: DateTime.UtcNow.AddDays(10)
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/meetings/{_meetingId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var meeting = await response.Content.ReadFromJsonAsync<MeetingResponse>(_jsonOptions);
        Assert.NotNull(meeting);
        Assert.Equal("Updated Meeting", meeting.Name);
    }

    [Fact]
    public async Task Update_Returns401_WhenMember()
    {
        // Arrange
        SetCurrentMember(_memberUuid);
        var updateDto = new MeetingUpdate(
            Name: "Hacked Meeting",
            Description: "Should not work",
            Location: "Nowhere",
            MeetingDateTime: DateTime.UtcNow.AddDays(10)
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/meetings/{_meetingId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204_WhenOwner()
    {
        // Arrange - create a meeting to delete
        SetCurrentMember(_ownerUuid);
        var createDto = new MeetingCreate(
            Name: "To Delete",
            Description: "Will be deleted",
            Location: "Delete Location",
            MeetingDateTime: DateTime.UtcNow.AddDays(30),
            CarnivalBlockId: _carnivalBlockId
        );
        var createResponse = await Client.PostAsJsonAsync("/api/v1/meetings", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<MeetingResponse>(_jsonOptions);
        Assert.NotNull(created);
        var toDeleteId = created.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/v1/meetings/{toDeleteId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns401_WhenMember()
    {
        // Act
        SetCurrentMember(_memberUuid);
        var response = await Client.DeleteAsync($"/api/v1/meetings/{_meetingId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
