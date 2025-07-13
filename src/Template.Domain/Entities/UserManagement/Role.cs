using Template.Domain.SeedWork;

namespace Template.Domain.Entities.UserManagement
{
    public class Role : EntityBase<int>, IUpdateAudit, ISoftDelete
    {
        public string Name { get; set; } = null!;

        public DateTime? UpdatedDate { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime? DeletedDate { get; private set; }

        // Relationships
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
