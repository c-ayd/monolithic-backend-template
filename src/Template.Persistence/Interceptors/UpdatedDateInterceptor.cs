using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Template.Domain.SeedWork;

namespace Template.Persistence.Interceptors
{
    public class UpdatedDateInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AddUpdatedDateToEntities(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AddUpdatedDateToEntities(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddUpdatedDateToEntities(DbContextEventData eventData)
        {
            var updatedEntries = eventData.Context?.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            if (updatedEntries == null || updatedEntries.Count == 0)
                return;

            foreach (var entry in updatedEntries)
            {
                if (entry.Entity is IUpdateAudit)
                {
                    entry.CurrentValues[nameof(IUpdateAudit.UpdatedDate)] = DateTime.UtcNow;
                }
                else
                {
                    entry.State = EntityState.Unchanged;
                }
            }
        }
    }
}
