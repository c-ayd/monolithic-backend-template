using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Test.Unit.Application.Validations.Extensions
{
    public class GuidValidationTest
    {
        private readonly Validator _validator;

        public GuidValidationTest()
        {
            _validator = new Validator();
        }

        [Fact]
        public void GuidValidation_WhenGuidIsValid_ShouldReturnNoError()
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Id = Guid.NewGuid()
            };

            // Act
            var result = _validator.Validate(obj);

            // Assert
            if (result.Errors.Count > 0)
            {
                Assert.Fail($"{obj.Id} did not pass the validation.");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public void GuidValidation_WhenGuidIsInvalid_ShouldReturnError(string? id)
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Id = id == null ? null : new Guid(id)
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
                RuleFor(_ => _.Id)
                    .GuidValidation();
            }
        }

        private class ValidationObject
        {
            public Guid? Id { get; set; }
        }
    }
}
