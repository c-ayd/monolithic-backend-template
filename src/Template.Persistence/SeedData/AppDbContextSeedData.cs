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

            // NOTE: Use the code above to access the database. Then check if there is any data
            // in the tables you want to seed data into. Depending on that, continue and seed data or return.
        }
    }
}
