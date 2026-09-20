using Cognitask.Api.Entities;

namespace Cognitask.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(string email);

    Task<bool> EmailExistsAsync(string email);

    Task AddAsync(User user);

    Task AddRefreshTokenAsync(RefreshToken refreshToken);

    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);

    Task RevokeRefreshTokenAsync(RefreshToken refreshToken);

    Task SaveChangesAsync();
}