using Template.Domain.SeedWork;

namespace Template.Domain.Shared.ValueObjects
{
    public class Address : ValueObjectBase
    {
        public string? Street { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return PostalCode;
            yield return City;
            yield return Country;
        }
    }
}
