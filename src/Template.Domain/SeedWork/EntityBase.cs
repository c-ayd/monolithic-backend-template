namespace Template.Domain.SeedWork
{
    public abstract class EntityBase<T> : IEquatable<EntityBase<T>>
        where T : notnull
    {
        public T Id { get; protected set; }
        public DateTime CreatedDate { get; private set; }

        public bool Equals(EntityBase<T>? other)
        {
            if (other is null)
                return false;

            return Id.Equals(other.Id);
        }

        public static bool operator== (EntityBase<T>? left, EntityBase<T>? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator!= (EntityBase<T>? left, EntityBase<T>? right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is EntityBase<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
