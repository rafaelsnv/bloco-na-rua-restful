using System.Net;
using System.Net.Http.Headers;
using BlocoNaRua.Tests.Infrastructure;

namespace BlocoNaRua.Tests.Integration;

public class MediaApiIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task UploadImage_ReturnsUnauthorized_WhenNoMemberIsResolved()
    {
        // The current identity points to a member that does not exist in the database.
        SetCurrentMember(Guid.NewGuid());

        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(new byte[10]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(file, "file", "photo.jpg");

        var response = await Client.PostAsync("/api/v1/media/image", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
