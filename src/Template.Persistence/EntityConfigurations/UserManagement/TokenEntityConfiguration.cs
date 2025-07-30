using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.Generators;

namespace Template.Persistence.EntityConfigurations.UserManagement
{
    public class TokenEntityConfiguration : IEntityTypeConfiguration<Token>
    {
        public void Configure(EntityTypeBuilder<Token> builder)
        {
            builder.Property(t => t.Id)
                .HasValueGenerator<GuidIdGenerator>();

            builder.HasIndex(t => t.ValueHashed)
                .IsUnique();
        }
    }
}
