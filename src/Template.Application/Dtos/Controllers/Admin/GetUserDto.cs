using Template.Application.Dtos.Entities.UserManagement;

namespace Template.Application.Dtos.Controllers.Admin
{
    public class GetUserDto
    {
        public required UserDto User { get; set; }
        public SecurityStateDto? SecurityState { get; set; }
        public required List<RoleDto> Roles { get; set; }
        public required List<LoginDto> Logins { get; set; }
    }
}
