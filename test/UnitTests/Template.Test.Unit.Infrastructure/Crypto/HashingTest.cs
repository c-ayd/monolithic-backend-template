using Cayd.Test.Generators;
using System.Reflection;
using System.Security.Cryptography;
using Template.Application.Dtos.Crypto.Enums;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Crypto.Structs;

namespace Template.Test.Unit.Infrastructure.Crypto
{
    public class HashingTest
    {
        private readonly Hashing _hashing;

        public HashingTest()
        {
            _hashing = new Hashing();
        }

        [Fact]
        public void HashPassword_WhenPasswordIsValid_ShouldHashPassword()
        {
            // Act
            var password = PasswordGenerator.Generate();
            var hashedPassword = _hashing.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
        }

        [Fact]
        public void HashPassword_WhenPasswordIsInvalid_ShouldThrowException()
        {
            // Act
            var result = Record.Exception(() =>
            {
                _hashing.HashPassword(string.Empty);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }

        [Fact]
        public void VerifyPassword_WhenHashingVersionIsNotFound_ShouldReturnVersionNotFound()
        {
            // Arrange
            var password = PasswordGenerator.Generate();
            var hashedPassword = _hashing.HashPassword(password);
            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            hashedPasswordBytes[0] = 0;
            hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            // Act
            var result = _hashing.VerifyPassword(hashedPassword, password);

            // Assert
            Assert.Equal(EPasswordVerificationResult.VersionNotFound, result);
        }

        [Fact]
        public void VerifyPassword_WhenHashingByteLengthsAreDifferent_ShouldReturnLengthMismatch()
        {
            // Arrange
            var password = PasswordGenerator.Generate();
            var hashedPassword = _hashing.HashPassword(password);
            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            hashedPasswordBytes[0] = byte.MaxValue;
            hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            var hashingVersionsField = typeof(Hashing).GetField("_hashingVersions", BindingFlags.NonPublic | BindingFlags.Static)!;
            var hashingVersions = (Dictionary<byte, HashingVersion>)hashingVersionsField.GetValue(_hashing)!;
            hashingVersions.Add(byte.MaxValue, new HashingVersion(
                HashAlgorithmName.SHA512,
                saltSize: 100,
                keySize: 100,
                iterations: 100
                ));

            // Act
            var result = _hashing.VerifyPassword(hashedPassword, password);

            // Assert
            Assert.Equal(EPasswordVerificationResult.LengthMismatch, result);
        }

        [Fact]
        public void VerifyPassword_WhenPasswordIsDifferent_ShouldReturnFail()
        {
            // Arrange
            var password = PasswordGenerator.Generate();
            var hashedPassword = _hashing.HashPassword(password);

            // Act
            var result = _hashing.VerifyPassword(hashedPassword, password + "a");

            // Assert
            Assert.Equal(EPasswordVerificationResult.Fail, result);
        }

        [Fact]
        public void VerifyPassword_WhenPasswordIsSameButVersionIsDifferent_ShouldReturnSuccessRehashNeeded()
        {
            // Arrange
            var password = PasswordGenerator.Generate();
            var hashedPassword = _hashing.HashPassword(password);
            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            hashedPasswordBytes[0] = byte.MaxValue - 1;
            hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            var hashingVersionsField = typeof(Hashing).GetField("_hashingVersions", BindingFlags.NonPublic | BindingFlags.Static)!;
            var hashingVersions = (Dictionary<byte, HashingVersion>)hashingVersionsField.GetValue(_hashing)!;

            var currentVersionField = typeof(Hashing).GetField("_currentVersion", BindingFlags.NonPublic | BindingFlags.Static)!;
            var currentVersion = (byte)currentVersionField.GetValue(_hashing)!;

            var currentHashingVersion = hashingVersions[currentVersion];
            hashingVersions.Add(byte.MaxValue - 1, currentHashingVersion);

            // Act
            var result = _hashing.VerifyPassword(hashedPassword, password);

            // Assert
            Assert.Equal(EPasswordVerificationResult.SuccessRehashNeeded, result);
        }

        [Fact]
        public void VerifyPassword_WhenPasswordAndVersionAreSame_ShouldReturnSuccess()
        {
            // Arrange
            var password = PasswordGenerator.Generate();
            var hashedPassword = _hashing.HashPassword(password);

            // Act
            var result = _hashing.VerifyPassword(hashedPassword, password);

            // Assert
            Assert.Equal(EPasswordVerificationResult.Success, result);
        }

        [Theory]
        [InlineData("", "test")]
        [InlineData("test", "")]
        public void VerifyPassword_WhenInputIsInvalid_ShouldThrowException(string hashedPassword, string password)
        {
            // Act
            var result = Record.Exception(() =>
            {
                _hashing.VerifyPassword(hashedPassword, password);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }

        [Fact]
        public void HashSha256_WhenValueIsValid_ShouldHashValue()
        {
            // Act
            var value = StringGenerator.GenerateUsingAsciiChars(5);
            var valueHashed = _hashing.HashSha256(value);

            // Assert
            Assert.NotNull(valueHashed);
            Assert.NotEqual(valueHashed, value);
        }

        [Fact]
        public void HashSha256_WhenValueIsInvalid_ShouldThrowException()
        {
            // Act
            var result = Record.Exception(() =>
            {
                _hashing.HashSha256(string.Empty);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }

        [Fact]
        public void CompareSha256_WhenValueIsSame_ShouldReturnTrue()
        {
            // Arrange
            var value = StringGenerator.GenerateUsingAsciiChars(5);
            var valueHashed = _hashing.HashSha256(value);

            // Act
            var result = _hashing.CompareSha256(valueHashed, value);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CompareSha256_WhenValueIsDifferent_ShouldReturnFalse()
        {
            // Arrange
            var value = StringGenerator.GenerateUsingAsciiChars(5);
            var valueHashed = _hashing.HashSha256(value);

            // Act
            var result = _hashing.CompareSha256(valueHashed, value + "a");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("", "test")]
        [InlineData("test", "")]
        public void CompareSha256_WhenValueIsInvalid_ShouldThrowException(string valueHashed, string value)
        {
            // Act
            var result = Record.Exception(() =>
            {
                _hashing.CompareSha256(valueHashed, value);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }
    }
}
