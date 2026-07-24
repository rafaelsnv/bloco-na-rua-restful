using System.Net;
using System.Net.Http.Headers;
using System.Text;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace BlocoNaRua.Tests.Services;

public class AdminServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AdminService _service;

    public AdminServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _service = new AdminService(_httpClientFactoryMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task DeleteSignupAsync_ReturnsSuccess_WhenSupabaseReturns200()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient("SupabaseAdmin")).Returns(httpClient);

        _configurationMock.Setup(c => c["Supabase:Url"]).Returns("https://example.supabase.co");
        _configurationMock.Setup(c => c["Supabase:ServiceRoleKey"]).Returns("test-service-role-key");

        // Act
        var result = await _service.DeleteSignupAsync(uuid);

        // Assert
        Assert.True(result.Deleted);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSignupAsync_ReturnsFailure_WhenSupabaseReturns404()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var handlerMock = new Mock<HttpMessageHandler>();
        var errorContent = "{\"error\":\"User not found\"}";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(errorContent, Encoding.UTF8, "application/json")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient("SupabaseAdmin")).Returns(httpClient);

        _configurationMock.Setup(c => c["Supabase:Url"]).Returns("https://example.supabase.co");
        _configurationMock.Setup(c => c["Supabase:ServiceRoleKey"]).Returns("test-service-role-key");

        // Act
        var result = await _service.DeleteSignupAsync(uuid);

        // Assert
        Assert.False(result.Deleted);
        Assert.Contains("NotFound", result.ErrorMessage);
        Assert.Contains("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSignupAsync_ThrowsInvalidOperation_WhenConfigMissing()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        _configurationMock.Setup(c => c["Supabase:Url"]).Returns((string?)null);
        _configurationMock.Setup(c => c["Supabase:ServiceRoleKey"]).Returns("test-service-role-key");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteSignupAsync(uuid));

        Assert.Equal("Supabase:Url configuration is missing", exception.Message);
    }

    [Fact]
    public async Task DeleteSignupAsync_ThrowsInvalidOperation_WhenServiceRoleKeyMissing()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        _configurationMock.Setup(c => c["Supabase:Url"]).Returns("https://example.supabase.co");
        _configurationMock.Setup(c => c["Supabase:ServiceRoleKey"]).Returns((string?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteSignupAsync(uuid));

        Assert.Equal("Supabase:ServiceRoleKey configuration is missing", exception.Message);
    }
}
