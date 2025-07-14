using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities.UserManagement;

namespace Template.Persistence.EntityConfigurations.UserManagement
{
    public class TokenEntityConfiguration : IEntityTypeConfiguration<Token>
    {
        public void Configure(EntityTypeBuilder<Token> builder)
        {
            builder.HasIndex(t => t.Value)
                .IsUnique();
        }
    }
}
