using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.Member;
using BlocoNaRua.Restful.Services.Interfaces;

namespace BlocoNaRua.Restful.Services;

public class MemberService : IMemberService
{
    private readonly IMembersRepository _membersRepository;
    private readonly ILogger<MemberService> _logger;

    public MemberService(
        IMembersRepository membersRepository,
        ILogger<MemberService> logger)
    {
        _membersRepository = membersRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MemberEntity>> GetAllMembersAsync()
    {
        try
        {
            _logger.LogInformation("Buscando todos os membros");
            return await _membersRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar membros");
            throw;
        }
    }

    public async Task<MemberEntity?> GetMemberByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Buscando membro com ID: {Id}", id);
            return await _membersRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar membro com ID: {Id}", id);
            throw;
        }
    }

    public async Task<MemberEntity> CreateMemberAsync(MemberCreate createModel)
    {
        try
        {
            _logger.LogInformation("Criando novo membro: {Name}", createModel.Name);

            // Validações de negócio
            if (string.IsNullOrWhiteSpace(createModel.Name))
                throw new ArgumentException("Nome do membro é obrigatório");

            if (string.IsNullOrWhiteSpace(createModel.Email))
                throw new ArgumentException("Email do membro é obrigatório");

            if (string.IsNullOrWhiteSpace(createModel.Password))
                throw new ArgumentException("Senha do membro é obrigatória");

            // Verificar se email já existe (lógica de negócio)
            var existingMembers = await _membersRepository.GetAllAsync();
            if (existingMembers.Any(m => m.Email.Equals(createModel.Email, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Email já está em uso");

            // Criar entidade do membro
            var member = new MemberEntity(
                id: 0,
                name: createModel.Name,
                email: createModel.Email,
                password: HashPassword(createModel.Password), // Lógica de negócio: hash da senha
                phone: createModel.Phone,
                profileImage: createModel.ProfileImage
            );

            var result = await _membersRepository.AddAsync(member);

            _logger.LogInformation("Membro criado com sucesso. ID: {Id}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar membro");
            throw;
        }
    }

    public async Task<bool> UpdateMemberAsync(int id, Member updateModel)
    {
        try
        {
            _logger.LogInformation("Atualizando membro com ID: {Id}", id);

            if (updateModel.Id != id)
                throw new ArgumentException("ID do modelo não corresponde ao ID da URL");

            // Buscar membro existente
            var existingMember = await _membersRepository.GetByIdAsync(id);
            if (existingMember == null)
                throw new ArgumentException("Membro não encontrado");

            // Verificar se email já existe (se foi alterado)
            if (!string.IsNullOrWhiteSpace(updateModel.Email) && 
                !updateModel.Email.Equals(existingMember.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingMembers = await _membersRepository.GetAllAsync();
                if (existingMembers.Any(m => m.Email.Equals(updateModel.Email, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("Email já está em uso");
            }

            // Atualizar propriedades
            existingMember.Name = updateModel.Name ?? existingMember.Name;
            existingMember.Email = updateModel.Email ?? existingMember.Email;
            existingMember.Phone = updateModel.Phone ?? existingMember.Phone;
            existingMember.ProfileImage = updateModel.ProfileImage ?? existingMember.ProfileImage;
            
            // Hash da senha se foi fornecida
            if (!string.IsNullOrWhiteSpace(updateModel.Password))
            {
                existingMember.Password = HashPassword(updateModel.Password);
            }

            await _membersRepository.UpdateAsync(existingMember);

            _logger.LogInformation("Membro atualizado com sucesso. ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar membro com ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteMemberAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deletando membro com ID: {Id}", id);

            var member = await _membersRepository.GetByIdAsync(id);
            if (member == null)
                throw new ArgumentException("Membro não encontrado");

            await _membersRepository.DeleteAsync(member);

            _logger.LogInformation("Membro deletado com sucesso. ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar membro com ID: {Id}", id);
            throw;
        }
    }

    private string HashPassword(string password)
    {
        // Lógica de negócio: hash da senha
        // Em produção, use BCrypt ou similar
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
} 
