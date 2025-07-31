using Template.Application.Dtos.Controllers.Admin;
using Template.Application.Dtos.Entities.UserManagement;
using Template.Application.Features.Queries.Admin.GetUsers;
using Template.Application.Mappings.Entities.UserManagement;

namespace Template.Application.Mappings.Controllers.Admin
{
    public static partial class AdminMappings
    {
        public static List<GetUserDto> Map(GetUsersResponse response)
        {
            var result = new List<GetUserDto>();

            foreach (var user in response.Users)
            {
                var item = new GetUserDto()
                {
                    User = UserManagementMappings.Map(user),
                    SecurityState = UserManagementMappings.Map(user.SecurityState!),
                    Roles = new List<RoleDto>(),
                    Logins = new List<LoginDto>()
                };

                foreach (var role in user.Roles)
                {
                    item.Roles.Add(UserManagementMappings.Map(role));
                }

                foreach (var login in user.Logins)
                {
                    item.Logins.Add(UserManagementMappings.Map(login));
                }

                result.Add(item);
            }

            return result;
        }
    }
}
