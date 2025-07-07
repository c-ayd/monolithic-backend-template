using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Template.Domain.Entities.UserManagement;

namespace Template.Persistence.DbContexts
{
    public class AppDbContext : DbContext
    {
        // User management
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<SecurityState> SecurityStates { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
