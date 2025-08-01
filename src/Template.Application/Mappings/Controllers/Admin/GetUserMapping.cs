using Template.Application.Dtos.Controllers.Admin;
using Template.Application.Dtos.Entities.UserManagement;
using Template.Application.Features.Queries.Admin.GetUser;
using Template.Application.Mappings.Entities.UserManagement;

namespace Template.Application.Mappings.Controllers.Admin
{
    public static partial class AdminMappings
    {
        public static GetUserDto? Map(GetUserResponse? response)
        {
            if (response == null)
                return null;

            var user = UserManagementMappings.Map(response.User);
            var securityState = UserManagementMappings.Map(response.User.SecurityState!);

            var roles = new List<RoleDto>();
            foreach (var role in response.User.Roles)
            {
                roles.Add(UserManagementMappings.Map(role));
            }

            var logins = new List<LoginDto>();
            foreach (var login in response.User.Logins)
            {
                logins.Add(UserManagementMappings.Map(login));
            }

            return new GetUserDto()
            {
                User = user,
                SecurityState = securityState,
                Roles = roles,
                Logins = logins
            };
        }
    }
}
