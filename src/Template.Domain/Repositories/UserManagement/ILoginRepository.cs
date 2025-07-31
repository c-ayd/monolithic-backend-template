using Template.Domain.Entities.UserManagement;
using Template.Domain.SeedWork;

namespace Template.Domain.Repositories.UserManagement
{
    public interface ILoginRepository : IRepositoryBase
    {
        Task AddAsync(Login newLogin);
        Task<Login?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Login?> GetByUserIdAndHashedRefreshTokenAsync(Guid userId, string hashedRefreshToken, CancellationToken cancellationToken = default);
        Task<ICollection<Login>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ICollection<Login>> GetAllActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        void Delete(Login login);
        void DeleteAll(IEnumerable<Login> logins);

        /// <summary>
        /// Deletes a login whose ID is the same as a given ID from the database.
        /// <para>
        /// It skips EF Core's tracking system.
        /// </para>
        /// </summary>
        /// <param name="id">ID of the login to be deleted</param>
        /// <returns>Returns the number of affected rows.</returns>
        Task<int> DeleteByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes all logins related to a given user ID from the database.
        /// <para>
        /// It skips EF Core's tracking system.
        /// </para>
        /// </summary>
        /// <param name="userId">ID of the user to delete related logins</param>
        /// <returns>Returns the number of affected rows.</returns>
        Task<int> DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
