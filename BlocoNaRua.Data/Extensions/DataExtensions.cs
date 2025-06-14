using BlocoNaRua.Core.Models;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlocoNaRua.Data.Extensions;

public static class DataExtensions
{
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .EnableDetailedErrors();
        });

    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>))
                .AddScoped<IUsersRepository, UsersRepository>()
                .AddScoped<IMeetingsRepository, MeetingsRepository>()
                .AddScoped<ICarnivalBlocksRepository, CarnivalBlocksRepository>()
                .AddScoped<ICarnivalBlockUsersRepository, CarnivalBlockUsersRepository>();
        return services;
    }

}
