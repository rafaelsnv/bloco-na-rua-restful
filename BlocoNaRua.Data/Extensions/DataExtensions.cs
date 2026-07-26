using BlocoNaRua.Core.Models;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlocoNaRua.Data.Extensions;

public sealed class PascalCaseNameTranslator : Npgsql.INpgsqlNameTranslator
{
    public string TranslateMemberName(string clrName) => clrName;

    public string TranslateTypeName(string clrName) => clrName;
}

public static class DataExtensions
{
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var useInMemory = environment.EnvironmentName == "Testing" ||
                          Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "true";

        if (useInMemory)
        {
            return services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestingDb");
                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors()
                           .EnableSensitiveDataLogging()
                           .EnableThreadSafetyChecks();
                }
            });
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_SupabaseDB")
        ?? throw new InvalidOperationException("Connection string not found");

        return services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
                   {
                        npgsqlOptions.MapEnum<Domain.Enums.RolesEnum>("roles", null, new PascalCaseNameTranslator());
                   })
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                   .EnableServiceProviderCaching()
                   .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));

            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors()
                       .EnableSensitiveDataLogging()
                       .EnableThreadSafetyChecks();
            }
        });
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMembersRepository, MembersRepository>()
                .AddScoped<IMeetingsRepository, MeetingsRepository>()
                .AddScoped<ICarnivalBlocksRepository, CarnivalBlocksRepository>()
                .AddScoped<IMeetingPresencesRepository, MeetingPresencesRepository>()
                .AddScoped<ICarnivalBlockMembersRepository, CarnivalBlockMembersRepository>();
        return services;
    }

}
