using Cayd.Uuid;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Template.Persistence.Generators
{
    public class GuidIdGenerator : ValueGenerator<Guid>
    {
        public override bool GeneratesTemporaryValues => false;
        public override Guid Next(EntityEntry entry) => Uuid.V7.Generate();
    }
}
