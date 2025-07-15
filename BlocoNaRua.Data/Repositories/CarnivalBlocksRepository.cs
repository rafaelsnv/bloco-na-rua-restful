using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlocksRepository
(
    AppDbContext appContext,
    IMembersRepository membersRepository
) : RepositoryBase<CarnivalBlockEntity>(appContext), ICarnivalBlocksRepository
{
    private readonly IMembersRepository _membersRepository = membersRepository;
    new public async Task<CarnivalBlockEntity> AddAsync(CarnivalBlockEntity carnivalBlock)
    {
        var owner = await _membersRepository.GetByIdAsync(carnivalBlock.OwnerId)
            ?? throw new KeyNotFoundException("Member does not exist.");

        return await base.AddAsync(carnivalBlock);
    }

    new public async Task<bool> UpdateAsync(CarnivalBlockEntity carnivalBlock)
    {
        var existingCarnivalBlock = await GetByIdAsync(carnivalBlock.Id)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        existingCarnivalBlock.Name = carnivalBlock.Name ?? existingCarnivalBlock.Name;
        existingCarnivalBlock.CarnivalBlockImage = carnivalBlock.CarnivalBlockImage ?? existingCarnivalBlock.CarnivalBlockImage;

        return await base.UpdateAsync(existingCarnivalBlock);
    }
}
