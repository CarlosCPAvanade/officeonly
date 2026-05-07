using Application.Interfaces;
using Application.Options;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OnlyOfficeOptions>(configuration.GetSection(OnlyOfficeOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión DefaultConnection es obligatoria.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IOnlyOfficeService, OnlyOfficeService>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddHttpClient(nameof(OnlyOfficeService));

        return services;
    }
}
