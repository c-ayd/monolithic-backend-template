using Template.Domain.SeedWork;

namespace Template.Domain.Entities.UserManagement
{
    public class User : EntityBase<Guid>, IUpdateAudit, ISoftDelete
    {
        public string? Email { get; set; }

        public DateTime UpdatedDate { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime DeletedDate { get; private set; }

        // Relationships
        public SecurityState? SecurityState { get; set; }
    }
}
