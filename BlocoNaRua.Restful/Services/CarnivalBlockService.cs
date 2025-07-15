using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Services.Interfaces;

namespace BlocoNaRua.Restful.Services;

public class CarnivalBlockService : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository;
    private readonly ILogger<CarnivalBlockService> _logger;

    public CarnivalBlockService(
        ICarnivalBlocksRepository carnivalBlocksRepository,
        ICarnivalBlockMembersRepository carnivalBlockMembersRepository,
        ILogger<CarnivalBlockService> logger)
    {
        _carnivalBlocksRepository = carnivalBlocksRepository;
        _carnivalBlockMembersRepository = carnivalBlockMembersRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CarnivalBlockEntity>> GetAllBlocksAsync()
    {
        try
        {
            _logger.LogInformation("Buscando todos os blocos de carnaval");
            return await _carnivalBlocksRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar blocos de carnaval");
            throw;
        }
    }

    public async Task<CarnivalBlockEntity?> GetBlockByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Buscando bloco de carnaval com ID: {Id}", id);
            return await _carnivalBlocksRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar bloco de carnaval com ID: {Id}", id);
            throw;
        }
    }

    public async Task<CarnivalBlockEntity> CreateBlockAsync(CarnivalBlockCreate createModel)
    {
        try
        {
            _logger.LogInformation("Criando novo bloco de carnaval: {Name}", createModel.Name);

            // Validação de negócio
            if (string.IsNullOrWhiteSpace(createModel.Name))
                throw new ArgumentException("Nome do bloco é obrigatório");

            // Criar entidade do bloco
            var carnivalBlock = new CarnivalBlockEntity(
                id: 0,
                name: createModel.Name,
                ownerId: createModel.OwnerId,
                inviteCode: GenerateInviteCode(),
                managersInviteCode: GenerateManagersInviteCode(),
                carnivalBlockImage: createModel.CarnivalBlockImage
            );

            // Salvar bloco
            var result = await _carnivalBlocksRepository.AddAsync(carnivalBlock);

            // Criar membro owner automaticamente
            var ownerMember = new CarnivalBlockMembersEntity(
                id: 0,
                carnivalBlockId: result.Id,
                memberId: createModel.OwnerId,
                role: RolesEnum.Owner
            );

            await _carnivalBlockMembersRepository.AddAsync(ownerMember);

            _logger.LogInformation("Bloco de carnaval criado com sucesso. ID: {Id}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bloco de carnaval");
            throw;
        }
    }

    public async Task<bool> UpdateBlockAsync(int id, CarnivalBlockUpdate updateModel, int memberId)
    {
        try
        {
            _logger.LogInformation("Atualizando bloco de carnaval com ID: {Id}", id);

            // Verificar se o membro tem permissão
            var member = await _carnivalBlockMembersRepository.GetByIdAsync(memberId);
            if (member == null)
                throw new UnauthorizedAccessException("Membro não encontrado");

            if (member.Role != RolesEnum.Owner && member.Role != RolesEnum.Manager)
                throw new UnauthorizedAccessException("Membro não tem permissão para atualizar o bloco");

            // Buscar bloco existente
            var existingBlock = await _carnivalBlocksRepository.GetByIdAsync(id);
            if (existingBlock == null)
                throw new ArgumentException("Bloco não encontrado");

            // Atualizar propriedades
            existingBlock.Name = updateModel.Name;
            existingBlock.CarnivalBlockImage = updateModel.CarnivalBlockImage;

            await _carnivalBlocksRepository.UpdateAsync(existingBlock);

            _logger.LogInformation("Bloco de carnaval atualizado com sucesso. ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar bloco de carnaval com ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteBlockAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deletando bloco de carnaval com ID: {Id}", id);

            var block = await _carnivalBlocksRepository.GetByIdAsync(id);
            if (block == null)
                throw new ArgumentException("Bloco não encontrado");

            await _carnivalBlocksRepository.DeleteAsync(block);

            _logger.LogInformation("Bloco de carnaval deletado com sucesso. ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bloco de carnaval com ID: {Id}", id);
            throw;
        }
    }

    private string GenerateInviteCode()
    {
        // Lógica para gerar código de convite único
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }

    private string GenerateManagersInviteCode()
    {
        // Lógica para gerar código de convite para managers único
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
} 
