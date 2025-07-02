using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlockMembersRepository(AppDbContext appContext) : RepositoryBase<CarnivalBlockMembersEntity>(appContext), ICarnivalBlockMembersRepository;
