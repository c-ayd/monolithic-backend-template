using Microsoft.EntityFrameworkCore.Storage;
using Template.Application.Abstractions.UOW;
using Template.Domain.Repositories.UserManagement;
using Template.Persistence.DbContexts;
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

        private TInterface GetRepository<TInterface, TImplementation>(TInterface? repository, Func<AppDbContext, TImplementation> ctor)
            where TImplementation : TInterface
        {
            if (repository == null)
            {
                repository = ctor(_appDbContext);
            }

            return repository;
        }

        private IUserRepository? users = null;
        public IUserRepository Users => GetRepository(users, (appDbContext) => new UserRepository(appDbContext));

        private IRoleRepository? roles = null;
        public IRoleRepository Roles => GetRepository(roles, (appDbContext) => new RoleRepository(appDbContext));

        private ILoginRepository? logins = null;
        public ILoginRepository Logins => GetRepository(logins, (appDbContext) => new LoginRepository(appDbContext));

        private ITokenRepository? tokens = null;
        public ITokenRepository Tokens => GetRepository(tokens, (appDbContext) => new TokenRepository(appDbContext));
    }
}
