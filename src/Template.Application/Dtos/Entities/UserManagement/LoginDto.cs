using System.Net;

namespace Template.Application.Dtos.Entities.UserManagement
{
    public class LoginDto
    {
        public Guid Id { get; set; }

        public IPAddress? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public Guid UserId { get; set; }
    }
}
