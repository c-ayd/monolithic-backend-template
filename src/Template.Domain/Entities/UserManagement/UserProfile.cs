using Template.Domain.SeedWork;
using Template.Domain.Shared.ValueObjects;

namespace Template.Domain.Entities.UserManagement
{
    public class UserProfile : EntityBase<Guid>
    {
        public string? FullName { get; set; }
        public Address Address { get; set; } = null!;

        // Relationships
        public Guid UserId { get; set; }
    }
}
