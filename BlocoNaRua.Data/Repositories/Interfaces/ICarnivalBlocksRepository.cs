using BlocoNaRua.Core.Models;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface ICarnivalBlocksRepository : IRepositoryBase<CarnivalBlockEntity>
{
    Task<bool> UpdateAsync(int memberId, CarnivalBlockEntity entity);
}
