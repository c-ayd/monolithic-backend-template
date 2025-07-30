using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Application.Validations.Constants.Entities.UserManagement;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.Converters;
using Template.Persistence.Generators;

namespace Template.Persistence.EntityConfigurations.UserManagement
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Id)
                .HasValueGenerator<GuidIdGenerator>();

            builder.HasIndex(u => u.Email);

            builder.Property(u => u.Email)
                .HasMaxLength(UserConstants.EmailMaxLength)
                .HasConversion<ToLowerConverter>();

            // Relationships
            builder.HasOne(u => u.SecurityState)
                .WithOne()
                .HasForeignKey<SecurityState>(ss => ss.UserId);

            builder.HasMany(u => u.Roles)
                .WithMany(r => r.Users);

            builder.HasMany(u => u.Logins)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId);

            builder.HasMany(u => u.Tokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);
        }
    }
}
