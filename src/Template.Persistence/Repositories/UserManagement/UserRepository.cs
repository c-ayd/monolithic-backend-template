﻿using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Repositories.UserManagement;
using Template.Persistence.DbContexts;

namespace Template.Persistence.Repositories.UserManagement
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(User newUser)
            => await _appDbContext.AddAsync(newUser);

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _appDbContext.Users
                .Where(u => u.Id.Equals(id))
                .FirstOrDefaultAsync();

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await _appDbContext.Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

        public async Task<Guid?> GetIdByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var id = await _appDbContext.Users
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            return id == Guid.Empty ? null : id;
        }

        public async Task<string?> GetEmailByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _appDbContext.Users
                .Where(u => u.Id.Equals(id))
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

        public async Task<SecurityState?> GetSecurityStateByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _appDbContext.Users
                .Where(u => u.Id.Equals(id))
                .Select(u => u.SecurityState)
                .FirstOrDefaultAsync();

        public async Task<(Guid, SecurityState)?> GetSecurityStateByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _appDbContext.Users
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.Id,
                    u.SecurityState
                })
                .FirstOrDefaultAsync();

            return result == null ? null : (result.Id, result.SecurityState!);
        }

        public async Task<UserProfile?> GetUserProfileByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _appDbContext.Users
                .Where(u => u.Id.Equals(id))
                .Select(u => u.UserProfile)
                .FirstOrDefaultAsync();

        public async Task<(Guid, UserProfile)?> GetUserProfileByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _appDbContext.Users
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.Id,
                    u.UserProfile
                })
                .FirstOrDefaultAsync();

            return result == null ? null : (result.Id, result.UserProfile!);
        }

        public void Delete(User user)
            => _appDbContext.Remove(user);
    }
}
