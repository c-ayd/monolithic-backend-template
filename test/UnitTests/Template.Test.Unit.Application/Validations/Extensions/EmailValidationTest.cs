using Cayd.Test.Generators;
using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Test.Unit.Application.Validations.Extensions
{
    public class EmailValidationTest
    {
        private readonly Validator _validator;

        public EmailValidationTest()
        {
            _validator = new Validator();
        }

        [Fact]
        public void EmailValidation_WhenEmailIsValid_ShouldReturnNoError()
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Email = EmailGenerator.Generate()
            };

            // Act
            var result = _validator.Validate(obj);

            // Assert
            if (result.Errors.Count > 0)
            {
                Assert.Fail($"{obj.Email} did not pass the validation.");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("abc")]
        [InlineData("abc@abc")]
        [InlineData("abc@abc.")]
        public void EmailValidation_WhenEmailIsInvalid_ShouldReturnError(string? email)
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Email = email
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
                RuleFor(_ => _.Email)
                    .EmailValidation();
            }
        }

        private class ValidationObject
        {
            public string? Email { get; set; }
        }
    }
}
