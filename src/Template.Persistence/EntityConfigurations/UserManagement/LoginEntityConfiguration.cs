using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Application.Validations.Constants.Entities.UserManagement;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.Generators;

namespace Template.Persistence.EntityConfigurations.UserManagement
{
    public class LoginEntityConfiguration : IEntityTypeConfiguration<Login>
    {
        public void Configure(EntityTypeBuilder<Login> builder)
        {
            builder.Property(l => l.Id)
                .HasValueGenerator<GuidIdGenerator>();

            builder.HasIndex(l => l.RefreshToken)
                .IsUnique();

            builder.Property(l => l.DeviceInfo)
                .HasMaxLength(LoginConstants.DeviceInfoMaxLength);
        }
    }
}
