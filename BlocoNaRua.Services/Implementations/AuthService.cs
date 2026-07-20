using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlocoNaRua.Services.Implementations;

public class AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var supabaseUrl = _configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url configuration is missing");
        var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"] ?? throw new InvalidOperationException("Supabase:ServiceRoleKey configuration is missing");
        var client = _httpClientFactory.CreateClient("SupabaseAdmin");
        client.DefaultRequestHeaders.Add("apikey", serviceRoleKey);

        var body = new { grant_type = "password", email = request.Email, password = request.Password };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{supabaseUrl}/auth/v1/token?grant_type=password", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Login failed: {response.StatusCode} - {responseBody}");
        }

        var responseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);

        return new LoginResponse(
            AccessToken: responseJson.GetProperty("access_token").GetString() ?? "",
            RefreshToken: responseJson.GetProperty("refresh_token").GetString() ?? "",
            ExpiresIn: responseJson.GetProperty("expires_in").GetInt32(),
            TokenType: responseJson.GetProperty("token_type").GetString() ?? "bearer",
            UserId: Guid.Parse(responseJson.GetProperty("user").GetProperty("id").GetString()!)
        );
    }
}
