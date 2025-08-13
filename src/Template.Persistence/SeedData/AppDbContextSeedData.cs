using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Policies;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Persistence.Settings;

namespace Template.Persistence.SeedData
{
    public static class AppDbContextSeedData
    {
        public static async Task SeedDataAppDbContext(this IServiceProvider services, IConfiguration configuration)
        {
            /**
             * NOTE: This seed data method is an example to show how to add default roles and email addresses.
             * Depending on specific needs, seeding data logic and how the default values are read can be changed.
             */

            await using var scope = services.CreateAsyncScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var seedDataSettings = configuration.GetSection(SeedDataAppDbContextSettings.SettingsKey).Get<SeedDataAppDbContextSettings>()!;

            await appDbContext.Database.EnsureCreatedAsync();

            await using var transaction = await appDbContext.Database.BeginTransactionAsync();
            try
            {
                var roles = await AddDefaultRoles(appDbContext);
                if (roles.Count == 0)
                    return;

                var users = await AddAdminAccounts(appDbContext, roles, seedDataSettings);
                if (users.Count == 0)
                    return;

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task<List<Role>> AddDefaultRoles(AppDbContext appDbContext)
        {
            var roles = new List<Role>();
            if (appDbContext.Roles.Any())
                return roles;

            roles.Add(new Role() { Name = AdminPolicy.RoleName });

            await appDbContext.AddRangeAsync(roles);
            await appDbContext.SaveChangesAsync();

            return roles;
        }

        private static async Task<List<User>> AddAdminAccounts(AppDbContext appDbContext, List<Role> roles, SeedDataAppDbContextSettings seedDataSettings)
        {
            var users = new List<User>();
            if (appDbContext.Users.Any())
                return users;

            var adminRole = roles
                .Where(r => r.Name == AdminPolicy.RoleName)
                .FirstOrDefault();

            if (adminRole == null)
                return users;

            foreach (var email in seedDataSettings.AdminEmails)
            {
                users.Add(new User()
                {
                    Email = email,
                    SecurityState = new SecurityState()
                    {
                        IsEmailVerified = true,
                    },
                    Roles = new List<Role>()
                    {
                        adminRole
                    }
                });
            }

            await appDbContext.AddRangeAsync(users);
            await appDbContext.SaveChangesAsync();

            return users;
        }
    }
}
