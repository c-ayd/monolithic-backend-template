﻿using Template.Domain.Entities.UserManagement;
using Template.Domain.SeedWork;

namespace Template.Domain.Repositories.UserManagement
{
    public interface IUserRepository : IRepositoryBase
    {
        Task AddAsync(User newUser);

        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Guid?> GetIdByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<string?> GetEmailByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SecurityState?> GetSecurityStateByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(Guid, SecurityState)?> GetSecurityStateByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(Guid, UserProfile)?> GetUserProfileByEmailAsync(string email, CancellationToken cancellationToken = default);

        void Delete(User user);
    }
}
