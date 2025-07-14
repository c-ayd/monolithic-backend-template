using System.Net;
using Template.Domain.SeedWork;

namespace Template.Domain.Entities.UserManagement
{
    public class Login : EntityBase<Guid>, IUpdateAudit
    {
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
        public IPAddress? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }

        public DateTime? UpdatedDate { get; private set; }
        
        // Relationships
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}
