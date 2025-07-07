using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities.UserManagement;

namespace Template.Persistence.EntityConfigurations.UserManagement
{
    public class SecurityStateEntityConfiguration : IEntityTypeConfiguration<SecurityState>
    {
        public void Configure(EntityTypeBuilder<SecurityState> builder)
        {
        }
    }
}
