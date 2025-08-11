using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories;

public class MeetingsRepository(AppDbContext appContext) : RepositoryBase<MeetingEntity>(appContext), IMeetingsRepository
{
    public async Task<IList<MeetingEntity>> GetAllByBlockIdAsync(int blockId)
    {
        return await DbSet
            .Where(m => m.CarnivalBlockId == blockId)
            .ToListAsync();
    }
}
