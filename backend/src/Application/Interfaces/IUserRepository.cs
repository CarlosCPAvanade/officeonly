using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
