using Microsoft.EntityFrameworkCore.Storage;
using Template.Domain.Repositories.UserManagement;

namespace Template.Application.Abstractions.UOW
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
    }
}
