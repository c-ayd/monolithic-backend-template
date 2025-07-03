namespace Template.Domain.SeedWork
{
    public abstract class ValueObjectBase : IEquatable<ValueObjectBase>
    {
        public abstract IEnumerable<object?> GetEqualityComponents();

        public bool Equals(ValueObjectBase? other)
        {
            if (other is null)
                return false;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public static bool operator== (ValueObjectBase? left, ValueObjectBase? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator!= (ValueObjectBase? left, ValueObjectBase? right)
        {
            return !(left == right);
        }


        public override bool Equals(object? obj)
        {
            return obj is ValueObjectBase other && Equals(other);
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }
    }
}
