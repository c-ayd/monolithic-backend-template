namespace Template.Application.Dtos.Entities.UserManagement
{
    public class SecurityStateDto
    {
        public bool IsEmailVerified { get; set; }
        public int FailedAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? UnlockDate { get; set; }

        public Guid UserId { get; set; }
    }
}
