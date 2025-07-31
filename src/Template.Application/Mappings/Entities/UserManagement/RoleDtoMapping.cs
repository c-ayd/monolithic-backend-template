using Template.Application.Dtos.Entities.UserManagement;
using Template.Domain.Entities.UserManagement;

namespace Template.Application.Mappings.Entities.UserManagement
{
    public static partial class UserManagementMappings
    {
        public static RoleDto Map(Role role)
            => new RoleDto()
            {
                Id = role.Id,
                CreatedDate = role.CreatedDate,
                Name = role.Name
            };
    }
}
