using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BlocoNaRua.Tests.Controllers;

public class MediaControllerTests
{
    private const long MaxSizeBytes = 1024;

    private readonly Mock<IMembersService> _membersServiceMock = new();
    private readonly Mock<IMemberIdentityService> _memberIdentityServiceMock = new();
    private readonly Mock<IErrorLogger> _errorLoggerMock = new();

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Uploads:BasePath"] = Path.Combine(Path.GetTempPath(), "bnr-tests-uploads"),
                ["Uploads:MaxSizeBytes"] = MaxSizeBytes.ToString(),
            })
            .Build();

    private MediaController CreateController()
    {
        var member = new MemberEntity(7) { Name = "Test", Email = "test@example.com", Uuid = Guid.NewGuid() };

        _memberIdentityServiceMock.Setup(s => s.GetMemberIdAsync()).ReturnsAsync(member.Id);
        _membersServiceMock.Setup(s => s.GetByIdAsync(member.Id)).ReturnsAsync(member);
        _membersServiceMock
            .Setup(s => s.UpdateAsync(member.Id, member.Id, It.IsAny<MemberEntity>()))
            .ReturnsAsync(member);

        var imageUrlService = new ImageUrlService(BuildConfiguration(), _errorLoggerMock.Object);
        return new MediaController(imageUrlService, _memberIdentityServiceMock.Object, _membersServiceMock.Object);
    }

    private static IFormFile CreateFile(string fileName, int length)
    {
        var content = new byte[length];
        return new FormFile(new MemoryStream(content), 0, length, "file", fileName);
    }

    [Fact]
    public async Task UploadImage_ReturnsOk_WithUploadsPath()
    {
        var controller = CreateController();

        var result = await controller.UploadImage(CreateFile("photo.jpg", 10));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var path = okResult.Value?.GetType().GetProperty("path")?.GetValue(okResult.Value) as string;
        Assert.NotNull(path);
        Assert.StartsWith("/uploads/", path);
    }

    [Fact]
    public async Task UploadImage_ReturnsUnsupportedMediaType_WhenExtensionIsNotAllowed()
    {
        var controller = CreateController();

        var result = await controller.UploadImage(CreateFile("payload.exe", 10));

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, statusResult.StatusCode);
    }

    [Fact]
    public async Task UploadImage_ReturnsPayloadTooLarge_WhenFileExceedsMaxSize()
    {
        var controller = CreateController();

        var result = await controller.UploadImage(CreateFile("photo.jpg", (int)MaxSizeBytes + 1));

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, statusResult.StatusCode);
    }

    [Fact]
    public async Task UploadImage_ReturnsUnauthorized_WhenMemberIsNotResolved()
    {
        var controller = CreateController();

        // No member behind the current identity: the identity service refuses to resolve one.
        _memberIdentityServiceMock
            .Setup(s => s.GetMemberIdAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Member not found from JWT claim."));

        var result = await controller.UploadImage(CreateFile("photo.jpg", 10));

        Assert.IsType<UnauthorizedResult>(result);
    }
}
