using Cayd.Test.Generators;
using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Test.Unit.Application.Validations.Extensions
{
    public class PasswordValidationTest
    {
        private readonly Validator _validator;

        public PasswordValidationTest()
        {
            _validator = new Validator();
        }

        [Fact]
        public void PasswordValidation_WhenPasswordIsValid_ShouldReturnNoError()
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = _validator.Validate(obj);

            // Assert
            if (result.Errors.Count > 0)
            {
                Assert.Fail($"{obj.Password} did not pass the validation.");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("123")]
        [InlineData("abc123")]
        [InlineData("abcefghijk")]
        [InlineData("1234567890")]
        public void PasswordValidation_WhenPasswordIsInvalid_ShouldReturnError(string? password)
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Password = password
            };

            // Act
            var result = _validator.Validate(obj);

            // Assert
            Assert.NotEmpty(result.Errors);
        }

        private class Validator : AbstractValidator<ValidationObject>
        {
            public Validator()
            {
                RuleFor(_ => _.Password)
                    .PasswordValidation();
            }
        }

        private class ValidationObject
        {
            public string? Password { get; set; }
        }
    }
}
