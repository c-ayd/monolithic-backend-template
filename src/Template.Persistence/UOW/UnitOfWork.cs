using Microsoft.EntityFrameworkCore.Storage;
using Template.Application.Abstractions.UOW;
using Template.Domain.Repositories.UserManagement;
using Template.Persistence.DbContexts;
using Template.Persistence.Exceptions;
using Template.Persistence.Repositories.UserManagement;

namespace Template.Persistence.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _appDbContext;

        public UnitOfWork(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => _appDbContext.Database.CommitTransactionAsync(cancellationToken);

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => _appDbContext.Database.RollbackTransactionAsync(cancellationToken);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _appDbContext.SaveChangesAsync(cancellationToken);

        private TInterface GetRepository<TInterface, TImplementation>(TInterface? repository)
            where TImplementation : TInterface
        {
            if (repository == null)
            {
                var repositoryType = typeof(TImplementation);
                var ctor = repositoryType.GetConstructor(new Type[] { typeof(AppDbContext) });
                if (ctor == null)
                    throw new RepositoryConstructorNotFoundException(repositoryType.Name);

                repository = (TImplementation)ctor.Invoke(new object[] { _appDbContext });
            }

            return repository;
        }

        private IUserRepository? users = null;
        public IUserRepository Users => GetRepository<IUserRepository, UserRepository>(users);

        private IRoleRepository? roles = null;
        public IRoleRepository Roles => GetRepository<IRoleRepository, RoleRepository>(roles);

        private ILoginRepository? logins = null;
        public ILoginRepository Logins => GetRepository<ILoginRepository, LoginRepository>(logins);

        private ITokenRepository? tokens = null;
        public ITokenRepository Tokens => GetRepository<ITokenRepository, TokenRepository>(tokens);
    }
}
