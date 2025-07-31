namespace Template.Application.Dtos.Entities.UserManagement
{
    public class RoleDto
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }

        public string Name { get; set; } = null!;
    }
}
