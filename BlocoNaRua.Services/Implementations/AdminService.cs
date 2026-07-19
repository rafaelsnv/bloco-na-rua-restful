using System.Net.Http.Headers;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlocoNaRua.Services.Implementations;

public class AdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IAdminService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<AdminDeleteResult> DeleteSignupAsync(Guid uuid)
    {
        var supabaseUrl = _configuration["Supabase:Url"] ?? "";
        var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"] ?? "";

        var client = _httpClientFactory.CreateClient("SupabaseAdmin");
        client.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", serviceRoleKey);

        var response = await client.DeleteAsync($"{supabaseUrl}/auth/v1/admin/users/{uuid}");

        if (response.IsSuccessStatusCode)
            return new AdminDeleteResult(true, null);

        var errorContent = await response.Content.ReadAsStringAsync();
        return new AdminDeleteResult(false, $"Failed to delete user: {response.StatusCode} - {errorContent}");
    }
}