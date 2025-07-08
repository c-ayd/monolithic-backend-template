using Template.Persistence.DbContexts;

namespace Template.Persistence.Exceptions
{
    public class RepositoryConstructorNotFoundException : Exception
    {
        public RepositoryConstructorNotFoundException(string repositoryName)
            : base($"{repositoryName} does not have a constructor taking {typeof(AppDbContext).Name} as a parameter.")
        {
        }
    }
}
