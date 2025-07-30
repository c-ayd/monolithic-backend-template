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
                dbContext.UntrackEntity(entity);
            }
        }

        public static void UntrackEntityCollection<T>(this DbContext dbContext, ICollection<T> collection)
        {
            foreach (var entity in collection)
            {
                dbContext.UntrackEntity(entity);
            }
        }
    }
}
