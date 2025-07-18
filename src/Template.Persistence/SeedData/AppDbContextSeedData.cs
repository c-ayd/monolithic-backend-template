using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Template.Persistence.DbContexts;

namespace Template.Persistence.SeedData
{
    public static class AppDbContextSeedData
    {
        public static async Task SeedDataAppDbContext(this WebApplication app)
        {
            //using var scope = app.Services.CreateAsyncScope();
            //var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // TODO: Use the code above to access the database. Then check if there is any data
            // in the tables you want to seed. Depending on that, continue and the seed data or return.
        }
    }
}
