using Template.Domain.Entities.UserManagement;

namespace Template.Application.Features.Queries.Admin.GetUser
{
    public class GetUserResponse
    {
        public required User User { get; set; }
    }
}
