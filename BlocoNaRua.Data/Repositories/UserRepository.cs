using AulaRepositoryPattern.Data.Repositories;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories;

public class UserRepository(AppDbContext appContext) : RepositoryBase<UserEntity>(appContext), IUserRepository
{
}
