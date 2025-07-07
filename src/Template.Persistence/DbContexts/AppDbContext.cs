using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.GlobalFilters;
using Template.Persistence.Interceptors;

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

            modelBuilder.ApplySoftDeleteFilter();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(new CreatedDateInterceptor());
            optionsBuilder.AddInterceptors(new UpdatedDateInterceptor());
            optionsBuilder.AddInterceptors(new SoftDeleteInterceptor());

            base.OnConfiguring(optionsBuilder);
        }
    }
}
