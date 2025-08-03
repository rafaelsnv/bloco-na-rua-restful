using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories;

public class MeetingsRepository(AppDbContext appContext) : RepositoryBase<MeetingEntity>(appContext), IMeetingsRepository;
