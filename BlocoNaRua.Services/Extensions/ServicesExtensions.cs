using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BlocoNaRua.Services.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICarnivalBlockService, CarnivalBlockService>()
                .AddScoped<IMembersService, MembersService>()
                .AddScoped<ICarnivalBlockMembersService, CarnivalBlockMembersService>()
                .AddScoped<IMeetingService, MeetingService>()
                .AddScoped<IMeetingPresenceService, MeetingPresenceService>()
                .AddScoped<IAuthorizationService, AuthorizationService>();
        return services;
    }
}
