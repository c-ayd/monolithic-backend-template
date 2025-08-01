using FluentValidation;
using Template.Application.Validations.Constants;
using Template.Application.Validations.Extensions;

namespace Template.Test.Unit.Application.Validations.Extensions
{
    public class PaginationValidationsTest
    {
        private readonly Validator _validator;

        public PaginationValidationsTest()
        {
            _validator = new Validator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(10)]
        public void PageValidation_WhenPageIsValid_ShouldReturnNoError(int? page)
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Page = page,
                PageSize = 10
            };

            // Act
            var result = _validator.Validate(obj);

            // Assert
            if (result.Errors.Count > 0)
            {
                Assert.Fail($"Page={obj.Page} did not pass the validation.");
            }
        }

        [Fact]
        public void PageValidation_WhenPageIsInvalid_ShouldReturnError()
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Page = -1,
                PageSize = 10
            };

            // Act
            var result = _validator.Validate(obj);

            // Assert
            Assert.NotEmpty(result.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(10)]
        public void PageSizeValidation_WhenPageIsValid_ShouldReturnNoError(int? pageSize)
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Page = 10,
                PageSize = pageSize
            };

            // Act
            var result = _validator.Validate(obj);

            // Assert
            if (result.Errors.Count > 0)
            {
                Assert.Fail($"PageSize={obj.PageSize} did not pass the validation.");
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(PaginationConstants.MaxPageSize + 1)]
        public void PageSizeValidation_WhenPageSizeIsInvalid_ShouldReturnError(int? pageSize)
        {
            // Arrange
            var obj = new ValidationObject()
            {
                Page = 10,
                PageSize = pageSize
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
                RuleFor(_ => _.Page)
                    .PageValidation();

                RuleFor(_ => _.PageSize)
                    .PageSizeValidation();
            }
        }

        private class ValidationObject
        {
            public int? Page { get; set; }
            public int? PageSize { get; set; }
        }
    }
}
