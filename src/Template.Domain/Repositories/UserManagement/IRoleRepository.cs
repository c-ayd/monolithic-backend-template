using Template.Domain.Entities.UserManagement;
using Template.Domain.SeedWork;

namespace Template.Domain.Repositories.UserManagement
{
    public interface IRoleRepository : IRepositoryBase
    {
        Task AddAsync(Role newRole);
        Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<ICollection<Role>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        void Delete(Role role);
    }
}
