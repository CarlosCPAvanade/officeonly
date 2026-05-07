using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }
}
