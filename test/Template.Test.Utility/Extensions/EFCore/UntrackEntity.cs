using Microsoft.EntityFrameworkCore;

namespace Template.Test.Utility.Extensions.EFCore
{
    public static partial class EFCoreExtensions
    {
        public static void UntrackEntity(this DbContext dbContext, object? entity)
        {
            if (entity == null)
                return;

            dbContext.Entry(entity).State = EntityState.Detached;
        }

        public static void UntrackEntities(this DbContext dbContext, params object?[] entities)
        {
            foreach (var entity in entities)
            {
                UntrackEntity(dbContext, entity);
            }
        }
    }
}
