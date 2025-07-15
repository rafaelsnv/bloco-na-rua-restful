using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;

namespace BlocoNaRua.Restful.Services.Interfaces;

public interface ICarnivalBlockService
{
    Task<IEnumerable<CarnivalBlockEntity>> GetAllBlocksAsync();
    Task<CarnivalBlockEntity?> GetBlockByIdAsync(int id);
    Task<CarnivalBlockEntity> CreateBlockAsync(CarnivalBlockCreate createModel);
    Task<bool> UpdateBlockAsync(int id, CarnivalBlockUpdate updateModel, int memberId);
    Task<bool> DeleteBlockAsync(int id);
} 
