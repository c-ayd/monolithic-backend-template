using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Entities.UserManagement.Enums;
using Template.Domain.Repositories.UserManagement;
using Template.Persistence.DbContexts;

namespace Template.Persistence.Repositories.UserManagement
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _appDbContext;

        public TokenRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(Token newToken)
            => await _appDbContext.Tokens.AddAsync(newToken);

        public async Task<Token?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _appDbContext.Tokens
                .Where(t => t.Id.Equals(id))
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<Token?> GetByValueAndPurposeAsync(string value, ETokenPurpose purpose, CancellationToken cancellationToken = default)
            => await _appDbContext.Tokens
                .Where(t => t.Value == value)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<ICollection<Token>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _appDbContext.Tokens
                .Where(t => t.UserId.Equals(userId))
                .ToListAsync(cancellationToken);

        public async Task<ICollection<Token>> GetAllByUserIdAndPurposeAsync(Guid userId, ETokenPurpose purpose, CancellationToken cancellationToken = default)
            => await _appDbContext.Tokens
                .Where(t => t.UserId.Equals(userId) &&
                    t.Purpose == purpose)
                .ToListAsync(cancellationToken);

        public void Delete(Token token)
            => _appDbContext.Tokens.Remove(token);

        public void DeleteAll(IEnumerable<Token> tokens)
            => _appDbContext.Tokens.RemoveRange(tokens);
    }
}
