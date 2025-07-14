using Template.Domain.Entities.UserManagement;
using Template.Domain.SeedWork;

namespace Template.Domain.Repositories.UserManagement
{
    public interface ILoginRepository : IRepositoryBase
    {
        Task AddAsync(Login newLogin);
        Task<Login?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Login?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<ICollection<Login>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        void Delete(Login login);
        void DeleteAll(IEnumerable<Login> logins);
    }
}
