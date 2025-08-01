using Template.Application.Dtos.Entities.UserManagement;
using Template.Application.Features.Queries.Admin.GetUsers;
using Template.Application.Mappings.Entities.UserManagement;

namespace Template.Application.Mappings.Controllers.Admin
{
    public static partial class AdminMappings
    {
        public static List<UserDto> Map(GetUsersResponse response)
        {
            var result = new List<UserDto>();

            foreach (var user in response.Users)
            {
                result.Add(UserManagementMappings.Map(user));
            }

            return result;
        }
    }
}
