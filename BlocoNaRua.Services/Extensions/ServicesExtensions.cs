using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BlocoNaRua.Services.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        // The password hasher is stateless, so a singleton is enough.
        services.AddSingleton<IPasswordHasher<MemberEntity>, PasswordHasher<MemberEntity>>();
        services.AddScoped<IErrorLogger, ApiErrorLoggerService>()
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<IImageUrlService, ImageUrlService>()
                .AddScoped<ICarnivalBlockService, CarnivalBlockService>()
                .AddScoped<IMembersService, MembersService>()
                .AddScoped<ICarnivalBlockMembersService, CarnivalBlockMembersService>()
                .AddScoped<IMeetingService, MeetingService>()
                .AddScoped<IMeetingPresenceService, MeetingPresenceService>()
                .AddScoped<IAdminService, AdminService>()
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IMemberIdentityService, MemberIdentityService>();
        return services;
    }
}
