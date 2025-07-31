using Template.Application.Dtos.Entities.UserManagement;
using Template.Application.Features.Queries.Authentication.GetLogins;
using Template.Application.Mappings.Entities.UserManagement;

namespace Template.Application.Mappings.Controllers.Authentication
{
    public static partial class AuthenticationMappings
    {
        public static List<LoginDto> Map(GetLoginsResponse response)
        {
            var result = new List<LoginDto>();

            foreach (var login in response.Logins)
            {
                result.Add(UserManagementMappings.Map(login));
            }

            return result;
        }
    }
}
