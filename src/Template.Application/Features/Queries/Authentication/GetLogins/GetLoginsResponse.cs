using Template.Domain.Entities.UserManagement;

namespace Template.Application.Features.Queries.Authentication.GetLogins
{
    public class GetLoginsResponse
    {
        public required ICollection<Login> Logins { get; set; }
    }
}
