using System.Text.Json;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api
{
    [Collection(nameof(TestHostCollection))]
    public class LocalizationTest
    {
        private readonly TestHostFixture _testHostFixture;

        public LocalizationTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
        }

        [Fact]
        public async Task LocalizationEndpoint_WhenGivenLanguageExists_ShouldReturnLocalizedText()
        {
            // Arrange
            _testHostFixture.UpdateAcceptLanguage("de-DE");

            // Act
            var response = await _testHostFixture.Client.GetAsync("/test/localization");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<string>(content);

            // Assert
            Assert.Equal("Verifizieren Sie Ihre E-Mail", result);

            _testHostFixture.ResetAcceptLanguage();
        }

        [Fact]
        public async Task LocalizationEndpoint_WhenGivenLanguageDoesNotExist_ShouldFallbackToEnglish()
        {
            // Arrange
            _testHostFixture.UpdateAcceptLanguage("es-ES");

            // Act
            var response = await _testHostFixture.Client.GetAsync("/test/localization");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<string>(content);

            // Assert
            Assert.Equal("Verify Your Email", result);

            _testHostFixture.ResetAcceptLanguage();
        }
    }
}
