using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Application.Validations.Constants.Entities.ValueObjects;
using Template.Domain.Entities.UserManagement;

namespace Template.Persistence.EntityConfigurations
{
    public class UserProfileEntityConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ComplexProperty(up => up.Address, address =>
                {
                    address.Property(a => a.Street)
                        .HasMaxLength(AddressConstants.StreetMaxLength);
                    address.Property(a => a.PostalCode)
                        .HasMaxLength(AddressConstants.PostalCodeMaxLength);
                    address.Property(a => a.City)
                        .HasMaxLength(AddressConstants.CityMaxLength);
                    address.Property(a => a.Country)
                        .HasMaxLength(AddressConstants.CountryMaxLength);
                });
        }
    }
}
