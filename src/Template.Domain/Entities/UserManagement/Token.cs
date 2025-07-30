using Template.Domain.Entities.UserManagement.Enums;
using Template.Domain.SeedWork;

namespace Template.Domain.Entities.UserManagement
{
    public class Token : EntityBase<Guid>
    {
        public string ValueHashed { get; set; } = null!;
        public ETokenPurpose Purpose { get; set; }
        public DateTime ExpirationDate { get; set; }

        // Relationships
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}
