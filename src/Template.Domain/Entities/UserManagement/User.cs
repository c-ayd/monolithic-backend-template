using Template.Domain.SeedWork;

namespace Template.Domain.Entities.UserManagement
{
    public class User : EntityBase<Guid>, IUpdateAudit, ISoftDelete
    {
        public string? Email { get; set; }

        public DateTime? UpdatedDate { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime? DeletedDate { get; private set; }

        // Relationships
        public SecurityState? SecurityState { get; set; }
        public UserProfile? UserProfile { get; set; }
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<Login> Logins { get; set; } = new List<Login>();
        public ICollection<Token> Tokens { get; set; } = new List<Token>();
    }
}
