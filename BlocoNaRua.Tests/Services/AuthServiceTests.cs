using System.Net;
using System.Text.Json;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace BlocoNaRua.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Supabase:Url"]).Returns("https://test.supabase.co");
        _configurationMock.Setup(c => c["Supabase:ServiceRoleKey"]).Returns("test-service-role-key");
        _service = new AuthService(_httpClientFactoryMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var userId = Guid.NewGuid();
        var responseJson = JsonSerializer.Serialize(new
        {
            access_token = "test_token",
            refresh_token = "refresh",
            expires_in = 3600,
            token_type = "bearer",
            user = new { id = userId.ToString() }
        });

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient("SupabaseAdmin")).Returns(httpClient);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test_token", result.AccessToken);
        Assert.Equal("refresh", result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal("bearer", result.TokenType);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task LoginAsync_ThrowsHttpRequestException_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest("invalid@example.com", "wrongpassword");

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\":\"Invalid login credentials\"}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient("SupabaseAdmin")).Returns(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _service.LoginAsync(request));
        Assert.Contains("Login failed", exception.Message);
    }
}
