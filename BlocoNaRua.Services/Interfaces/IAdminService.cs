namespace BlocoNaRua.Services.Interfaces;

public record AdminDeleteResult(bool Deleted, string? ErrorMessage);

public interface IAdminService
{
    Task<AdminDeleteResult> DeleteSignupAsync(Guid uuid);
}