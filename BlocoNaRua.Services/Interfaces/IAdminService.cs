namespace BlocoNaRua.Services.Interfaces;

public interface IAdminService
{
    Task<(bool Deleted, string? ErrorMessage)> DeleteSignupAsync(Guid uuid);
}