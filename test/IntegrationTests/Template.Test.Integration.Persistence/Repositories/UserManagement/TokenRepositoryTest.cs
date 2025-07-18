﻿using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Entities.UserManagement.Enums;
using Template.Persistence.DbContexts;
using Template.Persistence.Repositories.UserManagement;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Repositories.UserManagement
{
    [Collection(nameof(AppDbContextCollection))]
    public class TokenRepositoryTest
    {
        private readonly AppDbContext _appDbContext;

        private readonly TokenRepository _tokenRepository;

        public TokenRepositoryTest(AppDbContextFixture appDbContextFixture)
        {
            _appDbContext = appDbContextFixture.DbContext;

            _tokenRepository = new TokenRepository(_appDbContext);
        }

        [Fact]
        public async Task AddAsync_WhenTokenIsGiven_ShouldAddToken()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var token = new Token()
            {
                Value = StringGenerator.GenerateUsingAsciiChars(10),
                UserId = user.Id
            };

            // Act
            await _tokenRepository.AddAsync(token);
            await _appDbContext.SaveChangesAsync();

            // Assert
            var tokenId = token.Id;
            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(token);
            var result = await _appDbContext.Tokens
                .Where(t => t.Id.Equals(tokenId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTokenExists_ShouldReturnToken()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var token = new Token()
            {
                Value = StringGenerator.GenerateUsingAsciiChars(10),
                UserId = user.Id
            };
            await _tokenRepository.AddAsync(token);
            await _appDbContext.SaveChangesAsync();

            var tokenId = token.Id;
            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(token);

            // Act
            var result = await _tokenRepository.GetByIdAsync(tokenId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTokenDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _tokenRepository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByValueAndPurposeAsync_WhenTokenExists_ShouldReturnToken()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var tokenPurpose = ETokenPurpose.ResetPassword;
            var token = new Token()
            {
                Value = tokenValue,
                Purpose = tokenPurpose,
                UserId = user.Id
            };
            await _tokenRepository.AddAsync(token);
            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(token);

            // Act
            var result = await _tokenRepository.GetByValueAndPurposeAsync(tokenValue, tokenPurpose);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByValueAndPurposeAsync_WhenTokenDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _tokenRepository.GetByValueAndPurposeAsync(StringGenerator.GenerateUsingAsciiChars(10), ETokenPurpose.EmailVerification);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_WhenTokensRelatedToUserExist_ShouldReturnAllTokens()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user.Tokens = new List<Token>()
            {
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10) },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10) },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10) }
            };

            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _tokenRepository.GetAllByUserIdAsync(userId);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_WhenTokensRelatedToUserDoNotExist_ShouldReturnEmptyCollection()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _tokenRepository.GetAllByUserIdAsync(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_WhenUserDoesNotExist_ShouldReturnEmptyCollection()
        {
            // Act
            var result = await _tokenRepository.GetAllByUserIdAsync(Guid.NewGuid());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllByUserIdAndPurposeAsync_WhenTokensRelatedToUserAndPurposeExist_ShouldReturnAllRelatedTokens()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user.Tokens = new List<Token>()
            {
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.EmailVerification },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword }
            };

            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _tokenRepository.GetAllByUserIdAndPurposeAsync(userId, ETokenPurpose.ResetPassword);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllByUserIdAndPurposeAsync_WhenTokensRelatedToUserAndPurposeDoNotExist_ShouldReturnEmptyCollection()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user.Tokens = new List<Token>()
            {
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword }
            };

            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _tokenRepository.GetAllByUserIdAndPurposeAsync(userId, ETokenPurpose.EmailVerification);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllByUserIdAndPurposeAsync_WhenUserDoesNotExist_ShouldReturnEmptyCollection()
        {
            // Act
            var result = await _tokenRepository.GetAllByUserIdAndPurposeAsync(Guid.NewGuid(), ETokenPurpose.EmailVerification);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Delete_WhenTokenExists_ShouldDeleteToken()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var token = new Token()
            {
                Value = StringGenerator.GenerateUsingAsciiChars(10),
                UserId = user.Id
            };
            await _tokenRepository.AddAsync(token);
            await _appDbContext.SaveChangesAsync();

            var tokenId = token.Id;

            // Act
            _tokenRepository.Delete(token);
            await _appDbContext.SaveChangesAsync();

            // Assert
            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(token);
            var result = await _appDbContext.Tokens
                .Where(t => t.Id.Equals(tokenId))
                .FirstOrDefaultAsync();

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAll_WhenTokensExist_ShouldDeleteTokens()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user.Tokens = new List<Token>()
            {
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword },
                new Token() { Value = StringGenerator.GenerateUsingAsciiChars(10), Purpose = ETokenPurpose.ResetPassword }
            };

            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;

            // Act
            _tokenRepository.DeleteAll(user.Tokens);
            await _appDbContext.SaveChangesAsync();

            // Assert
            _appDbContext.UntrackEntities(user.Tokens.ToArray());
            _appDbContext.UntrackEntity(user);
            var result = await _appDbContext.Tokens
                .Where(t => t.UserId.Equals(userId))
                .ToListAsync();

            Assert.Empty(result);
        }
    }
}
