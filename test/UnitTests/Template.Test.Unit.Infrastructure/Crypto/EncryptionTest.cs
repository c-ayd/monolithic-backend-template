using Cayd.Test.Generators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Crypto;
using Template.Test.Utility;

namespace Template.Test.Unit.Infrastructure.Crypto
{
    public class EncryptionTest
    {
        private readonly Encryption _encryption;

        public EncryptionTest()
        {
            var config = ConfigurationHelper.CreateConfiguration();
            var aesSettings = config.GetSection(AesSettings.SettingsKey).Get<AesSettings>()!;

            _encryption = new Encryption(Options.Create(aesSettings));
        }

        [Fact]
        public void Encrypt_WhenValueIsValid_ShouldEncryptValue()
        {
            // Arrange
            var value = StringGenerator.GenerateUsingAsciiChars(10);

            // Act
            var result = _encryption.Encrypt(value);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(value, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Encrypt_WhenValueIsInvalid_ShouldThrowException(string? value)
        {
            // Act
            var result = Record.Exception(() =>
            {
                _encryption.Encrypt(value);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }

        [Fact]
        public void Decrypt_WhenEncryptedValueIsValidAndTypeIdIsSame_ShouldDecryptValue()
        {
            // Arrange
            var value = StringGenerator.GenerateUsingAsciiChars(10);
            var encryptedValue = _encryption.Encrypt(value);

            // Act
            var result = _encryption.Decrypt(encryptedValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(value, result);
        }

        [Fact]
        public void Decrypt_WhenEncryptedValueIsValidButTypeIdIsDifferent_ShouldReturnNull()
        {
            // Arrange
            var value = StringGenerator.GenerateUsingAsciiChars(10);
            var encryptedValue = _encryption.Encrypt(value);
            var encryptedValueBytes = Encoding.UTF8.GetBytes(encryptedValue);
            encryptedValueBytes[0] = byte.MaxValue;
            encryptedValue = Encoding.UTF8.GetString(encryptedValueBytes);

            // Act
            var result = _encryption.Decrypt(encryptedValue);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Decrypt_WhenEncryptedValueIsInvalid_ShouldThrowException(string? encryptedValue)
        {
            // Act
            var result = Record.Exception(() =>
            {
                _encryption.Decrypt(encryptedValue);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }

        [Fact]
        public void Compare_WhenEncryptedValueAndProvidedValueAreSame_ShouldReturnTrue()
        {
            // Arrange
            var value = StringGenerator.GenerateUsingAsciiChars(10);
            var encryptedValue = _encryption.Encrypt(value);

            // Act
            var result = _encryption.Compare(encryptedValue, value);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Compare_WhenEncryptedValueAndProvidedValueAreDifferent_ShouldReturnFalse()
        {
            // Arrange
            var value = StringGenerator.GenerateUsingAsciiChars(10);
            var encryptedValue = _encryption.Encrypt(value);

            // Act
            var result = _encryption.Compare(encryptedValue, value + "a");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Compare_WhenEncryptedValueIsInvalid_ShouldThrowException(string? encryptedValue)
        {
            // Act
            var result = Record.Exception(() =>
            {
                _encryption.Compare(encryptedValue, StringGenerator.GenerateUsingAsciiChars(10));
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Compare_WhenProvidedValueIsInvalid_ShouldThrowException(string? value)
        {
            // Arrange
            var encryptedValue = _encryption.Encrypt(StringGenerator.GenerateUsingAsciiChars(10));

            // Act
            var result = Record.Exception(() =>
            {
                _encryption.Compare(encryptedValue, value);
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }
    }
}
