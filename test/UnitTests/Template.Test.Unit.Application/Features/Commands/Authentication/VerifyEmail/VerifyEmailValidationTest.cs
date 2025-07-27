using Cayd.Test.Generators;
using Template.Application.Features.Commands.Authentication.VerifyEmail;

namespace Template.Test.Unit.Application.Features.Commands.Authentication.VerifyEmail
{
    public class VerifyEmailValidationTest
    {
        private readonly VerifyEmailValidation _validator;

        public VerifyEmailValidationTest()
        {
            _validator = new VerifyEmailValidation();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void VerifyEmailValidation_WhenTokenIsNullOrEmpty_ShouldReturnError(string? token)
        {
            // Arrange
            var request = new VerifyEmailRequest()
            {
                Token = token
            };

            // Act
            var result = _validator.Validate(request);

            // Arrange
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void VerifyEmailValidation_WhenTokenHasValue_ShouldReturnNoError()
        {
            // Arrange
            var request = new VerifyEmailRequest()
            {
                Token = StringGenerator.GenerateUsingAsciiChars(10)
            };

            // Act
            var result = _validator.Validate(request);

            // Arrange
            Assert.Empty(result.Errors);
        }
    }
}
