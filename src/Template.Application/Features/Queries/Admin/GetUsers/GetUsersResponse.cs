using Template.Domain.Entities.UserManagement;

namespace Template.Application.Features.Queries.Admin.GetUsers
{
    public class GetUsersResponse
    {
        public required List<User> Users { get; set; }
    }
}
