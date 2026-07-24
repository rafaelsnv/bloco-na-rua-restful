using System.Net.Http.Headers;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlocoNaRua.Services.Implementations;

public class AdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IAdminService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<(bool Deleted, string? ErrorMessage)> DeleteSignupAsync(Guid uuid)
    {
        var supabaseUrl = _configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url configuration is missing");
        var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"] ?? throw new InvalidOperationException("Supabase:ServiceRoleKey configuration is missing");

        var client = _httpClientFactory.CreateClient("SupabaseAdmin");
        client.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", serviceRoleKey);

        var response = await client.DeleteAsync($"{supabaseUrl}/auth/v1/admin/users/{uuid}");

        if (response.IsSuccessStatusCode)
            return (true, null);

        var errorContent = await response.Content.ReadAsStringAsync();
        return (false, $"Failed to delete user: {response.StatusCode} - {errorContent}");
    }
}