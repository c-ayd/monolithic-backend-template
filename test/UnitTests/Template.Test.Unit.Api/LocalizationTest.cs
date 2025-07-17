using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using Template.Test.Unit.Api.Collections;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Unit.Api
{
    [Collection(nameof(TestHostCollection))]
    public class LocalizationTest
    {
        private readonly IHost _host;
        private readonly HttpClient _client;

        public LocalizationTest(TestHostFixture testHostFixture)
        {
            _host = testHostFixture.TestHost;
            _client = testHostFixture.TestClient;
        }

        private void ReplaceAcceptLanguageHeader(string culture)
        {
            _client.DefaultRequestHeaders.AcceptLanguage.Clear();
            _client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        }

        private void ResetAcceptLanguageHeader(List<StringWithQualityHeaderValue> headerValue)
        {
            _client.DefaultRequestHeaders.AcceptLanguage.Clear();
            foreach (var item in headerValue)
            {
                _client.DefaultRequestHeaders.AcceptLanguage.Add(item);
            }
        }

        [Fact]
        public async Task LocalizationTestEndpoint_WhenGivenLanguageExists_ShouldReturnLocalizedText()
        {
            // Arrange
            var defaultAcceptLanguage = _client.DefaultRequestHeaders.AcceptLanguage.ToList();

            ReplaceAcceptLanguageHeader("de-DE");

            // Act
            var response = await _client.GetAsync("/localization-test");
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("Verifizieren Sie Ihre E-Mail", result);

            ResetAcceptLanguageHeader(defaultAcceptLanguage);
        }

        [Fact]
        public async Task LocalizationTestEndpoint_WhenGivenLanguageDoesNotExist_ShouldFallbackToEnglish()
        {
            // Arrange
            var defaultAcceptLanguage = _client.DefaultRequestHeaders.AcceptLanguage.ToList();

            ReplaceAcceptLanguageHeader("es-ES");

            // Act
            var response = await _client.GetAsync("/localization-test");
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("Verify Your Email", result);

            ResetAcceptLanguageHeader(defaultAcceptLanguage);
        }
    }
}
