using Template.Application.Dtos.Entities.UserManagement;
using Template.Domain.Entities.UserManagement;

namespace Template.Application.Mappings.Entities.UserManagement
{
    public static partial class UserManagementMappings
    {
        public static LoginDto Map(Login login)
            => new LoginDto()
            {
                IpAddress = login.IpAddress,
                DeviceInfo = login.DeviceInfo,
                UpdatedDate = login.UpdatedDate,
                UserId = login.UserId
            };
    }
}
