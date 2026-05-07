using Application.Interfaces;
using Application.Options;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Data;

public class DbInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedOptions _seedOptions;

    public DbInitializer(AppDbContext dbContext, IPasswordHasher passwordHasher, IOptions<SeedOptions> seedOptions)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _seedOptions = seedOptions.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        if (!await _dbContext.Roles.AnyAsync(cancellationToken))
        {
            var roles = new[]
            {
                new Role { Id = Guid.NewGuid(), Name = "admin" },
                new Role { Id = Guid.NewGuid(), Name = "editor" },
                new Role { Id = Guid.NewGuid(), Name = "reader" }
            };

            await _dbContext.Roles.AddRangeAsync(roles, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await _dbContext.Users.AnyAsync(x => x.UserName == _seedOptions.AdminUserName, cancellationToken))
        {
            var adminRole = await _dbContext.Roles.FirstAsync(x => x.Name == "admin", cancellationToken);
            await _dbContext.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                UserName = _seedOptions.AdminUserName,
                Email = _seedOptions.AdminEmail,
                PasswordHash = _passwordHasher.Hash(_seedOptions.AdminPassword),
                RoleId = adminRole.Id,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
