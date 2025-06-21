using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlockUsersRepository(AppDbContext appContext) : RepositoryBase<CarnivalBlockUsersEntity>(appContext), ICarnivalBlockUsersRepository
{
}
