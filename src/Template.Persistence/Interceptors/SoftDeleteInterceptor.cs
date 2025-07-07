using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Template.Domain.SeedWork;

namespace Template.Persistence.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            MarkEntitiesAsDeleted(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            MarkEntitiesAsDeleted(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void MarkEntitiesAsDeleted(DbContextEventData eventData)
        {
            var deletedEntries = eventData.Context?.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            if (deletedEntries == null || deletedEntries.Count == 0)
                return;

            foreach (var entry in deletedEntries)
            {
                if (entry.Entity is ISoftDelete)
                {
                    entry.CurrentValues[nameof(ISoftDelete.IsDeleted)] = true;
                    entry.CurrentValues[nameof(ISoftDelete.DeletedDate)] = DateTime.UtcNow;

                    entry.State = EntityState.Modified;
                }
            }
        }
    }
}
