using Template.Domain.Entities.UserManagement;
using Template.Domain.Entities.UserManagement.Enums;
using Template.Domain.SeedWork;

namespace Template.Domain.Repositories.UserManagement
{
    public interface ITokenRepository : IRepositoryBase
    {
        Task AddAsync(Token newToken);
        Task<Token?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Token?> GetByValueAndPurposeAsync(string value, ETokenPurpose purpose, CancellationToken cancellationToken = default);
        Task<ICollection<Token>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ICollection<Token>> GetAllByUserIdAndPurposeAsync(Guid userId, ETokenPurpose purpose, CancellationToken cancellationToken = default);
        void Delete(Token token);
        void DeleteAll(IEnumerable<Token> tokens);
    }
}
