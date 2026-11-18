using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class ApiErrorEntity(int id) : EntityBase(id)
{
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? AdditionalData { get; set; }
}
