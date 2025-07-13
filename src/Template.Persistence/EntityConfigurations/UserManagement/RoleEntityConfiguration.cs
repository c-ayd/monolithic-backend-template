using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Application.Validations.Constants.Entities.UserManagement;
using Template.Domain.Entities.UserManagement;

namespace Template.Persistence.EntityConfigurations.UserManagement
{
    public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(r => r.Name)
                .HasMaxLength(RoleConstants.NameMaxLength);
        }
    }
}
