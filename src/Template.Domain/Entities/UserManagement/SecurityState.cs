using Template.Domain.SeedWork;

namespace Template.Domain.Entities.UserManagement
{
    public class SecurityState : EntityBase<Guid>, IUpdateAudit
    {
        public string? PasswordHashed { get; set; }
        public bool IsEmailVerified { get; set; }
        public int FailedAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? UnlockDate { get; set; }

        public DateTime? UpdatedDate { get; private set; }

        // Relationships
        public Guid UserId { get; set; }
    }
}
