using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class CarnivalBlockMembersService
(
    ICarnivalBlockMembersRepository repository,
    IMembersRepository membersRepository,
    ICarnivalBlocksRepository carnivalBlocksRepository
) : ICarnivalBlockMembersService
{
    private readonly ICarnivalBlockMembersRepository _repository = repository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;

    public async Task<List<CarnivalBlockMembersEntity>> GetAllAsync()
    {
        return (await _repository.GetAllAsync()).ToList();
    }

    public async Task<CarnivalBlockMembersEntity?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task CreateAsync(CarnivalBlockMembersEntity carnivalBlockMember)
    {
        var member = await _membersRepository.GetByIdAsync(carnivalBlockMember.MemberId)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockMember.CarnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        await _repository.AddAsync(carnivalBlockMember);
    }
}
