using Template.Infrastructure.Crypto;
using Template.Infrastructure.Crypto.Exceptions;

namespace Template.Test.Unit.Infrastructure.Crypto
{
    public class TokenGeneratorTest
    {
        private readonly TokenGenerator _tokenGenerator;

        public TokenGeneratorTest()
        {
            _tokenGenerator = new TokenGenerator();
        }

        [Theory]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(128)]
        public void Generate_WhenDataLengthIsCorrect_ShouldGenerateToken(int dataLength)
        {
            // Act
            var result = _tokenGenerator.Generate(dataLength);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0, "The length of the token is zero.");
        }

        [Fact]
        public void Generate_WhenDataLengthIsZero_ShouldReturnEmptyString()
        {
            // Act
            var result = _tokenGenerator.Generate(0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length == 0, "The length of the token is not zero.");
        }

        [Fact]
        public void Generate_WhenDataLengthIsNegative_ShouldThrowException()
        {
            // Act
            var result = Record.Exception(() =>
            {
                _tokenGenerator.Generate(-1);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<InvalidDataLengthException>(result);
        }

        [Theory]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(128)]
        public void GenerateBase64UrlSafe_WhenItIsCalled_ShouldGenerateTokenThatIsUrlSafe(int dataLength)
        {
            // Act
            var result = _tokenGenerator.GenerateBase64UrlSafe(dataLength);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0, "The length of the token is zero.");
            Assert.Matches(@"^[a-zA-Z0-9\-_.~]*$", result);
        }

        [Fact]
        public void GenerateBase64UrlSafe_WhenDataLengthIsZero_ShouldReturnEmptyString()
        {
            // Act
            var result = _tokenGenerator.GenerateBase64UrlSafe(0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length == 0, "The length of the token is not zero.");
        }

        [Fact]
        public void GenerateBase64UrlSafe_WhenDataLengthIsNegative_ShouldThrowException()
        {
            // Act
            var result = Record.Exception(() =>
            {
                _tokenGenerator.GenerateBase64UrlSafe(-1);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<InvalidDataLengthException>(result);
        }
    }
}
