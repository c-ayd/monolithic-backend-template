using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Application.Validations.Constants.Entities.UserManagement;
using Template.Domain.Entities.UserManagement;

namespace Template.Persistence.EntityConfigurations.UserManagement
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Email);

            builder.Property(u => u.Email)
                .HasMaxLength(UserConstants.EmailMaxLength);

            // Relationships
            builder.HasOne(u => u.SecurityState)
                .WithOne()
                .HasForeignKey<SecurityState>(ss => ss.UserId);

            builder.HasOne(u => u.UserProfile)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId);
        }
    }
}
