namespace Template.Application.Dtos.Entities.UserManagement
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? Email { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
