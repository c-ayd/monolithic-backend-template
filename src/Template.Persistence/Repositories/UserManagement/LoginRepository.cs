using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Repositories.UserManagement;
using Template.Persistence.DbContexts;

namespace Template.Persistence.Repositories.UserManagement
{
    public class LoginRepository : ILoginRepository
    {
        private readonly AppDbContext _appDbContext;

        public LoginRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(Login newLogin)
            => await _appDbContext.Logins.AddAsync(newLogin);

        public async Task<Login?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _appDbContext.Logins
                .Where(l => l.Id.Equals(id))
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<Login?> GetByUserIdAndHashedRefreshTokenAsync(Guid userId, string hashedRefreshToken, CancellationToken cancellationToken = default)
            => await _appDbContext.Logins
                .Where(l => l.UserId.Equals(userId) &&
                    l.RefreshTokenHashed == hashedRefreshToken)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<ICollection<Login>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _appDbContext.Logins
                .Where(l => l.UserId.Equals(userId))
                .ToListAsync(cancellationToken);

        public void Delete(Login login)
            => _appDbContext.Logins.Remove(login);

        public void DeleteAll(IEnumerable<Login> logins)
            => _appDbContext.Logins.RemoveRange(logins);

        public async Task<int> DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _appDbContext.Logins
                .Where(l => l.UserId.Equals(userId))
                .ExecuteDeleteAsync(cancellationToken);
    }
}
