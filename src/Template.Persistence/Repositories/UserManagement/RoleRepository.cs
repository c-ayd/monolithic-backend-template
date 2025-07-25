﻿using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Repositories.UserManagement;
using Template.Persistence.DbContexts;

namespace Template.Persistence.Repositories.UserManagement
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _appDbContext;

        public RoleRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(Role newRole)
            => await _appDbContext.Roles.AddAsync(newRole);

        public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _appDbContext.Roles
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            => await _appDbContext.Roles
                .Where(r => r.Name == name)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<ICollection<Role>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (page < 0 || pageSize < 0)
                return Array.Empty<Role>();

            return await _appDbContext.Roles
                .OrderByDescending(r => r.CreatedDate)
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public void Delete(Role role)
            => _appDbContext.Roles.Remove(role);
    }
}
