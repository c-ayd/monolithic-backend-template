namespace Template.Domain.SeedWork
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; }
        DateTime DeletedDate { get; }
    }
}
