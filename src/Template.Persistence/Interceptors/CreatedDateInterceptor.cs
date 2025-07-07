using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Template.Domain.SeedWork;

namespace Template.Persistence.Interceptors
{
    public class CreatedDateInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AddCreatedDateToEntities(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AddCreatedDateToEntities(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddCreatedDateToEntities(DbContextEventData eventData)
        {
            var addedEntries = eventData.Context?.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .ToList();

            if (addedEntries == null || addedEntries.Count == 0)
                return;

            foreach (var entry in addedEntries)
            {
                var entityType = entry.Entity.GetType().BaseType;
                if (entityType != null &&
                    entityType.IsGenericType &&
                    entityType.GetGenericTypeDefinition() == typeof(EntityBase<>))
                {
                    entry.CurrentValues[nameof(EntityBase<object>.CreatedDate)] = DateTime.UtcNow;
                }
            }
        }
    }
}
