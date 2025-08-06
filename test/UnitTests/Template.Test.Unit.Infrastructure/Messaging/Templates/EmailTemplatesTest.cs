using Cayd.Test.Generators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Globalization;
using Template.Infrastructure.Messaging.Templates;
using Template.Infrastructure.Settings.Messaging.Templates;
using Template.Test.Utility;

namespace Template.Test.Unit.Infrastructure.Messaging.Templates
{
    public class EmailTemplatesTest
    {
        private readonly EmailTemplates _emailTemplates;
        private readonly TemplateLinksSettings _templateLinksSettings;

        public EmailTemplatesTest()
        {
            var config = ConfigurationHelper.CreateConfiguration();
            _templateLinksSettings = config.GetSection(TemplateLinksSettings.SettingsKey).Get<TemplateLinksSettings>()!;

            var localizationOptions = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
            var resourceFactory = new ResourceManagerStringLocalizerFactory(localizationOptions, NullLoggerFactory.Instance);
            var localizer = new StringLocalizer<EmailTemplates>(resourceFactory);

            _emailTemplates = new EmailTemplates(localizer, Options.Create(_templateLinksSettings));
        }

        [Fact]
        public void GetAnyTemplate_WhenGivenLanguageExists_ShouldReturnLocalizedText()
        {
            // Arrange
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

            var token = StringGenerator.GenerateUsingAsciiChars(10);
            var expirationTimeInHours = Random.Shared.Next();

            // Act
            var result = _emailTemplates.GetEmailVerificationTemplate(token, expirationTimeInHours);

            // Assert
            Assert.NotNull(result.Subject);
            Assert.Equal("Verifizieren Sie Ihre E-Mail", result.Subject);
            Assert.NotNull(result.Body);

            var body = $"Bitte klicken Sie auf den unten stehenden Link, um Ihre E-Mail zu bestätigen.\n\n" +
                $"{_templateLinksSettings.EmailVerification}{token}\n\n" +
                $"Der Link ist für {expirationTimeInHours} Stunden gültig.";
            Assert.Equal(body, result.Body.Replace("\r\n", "\n"));
        }

        [Fact]
        public void GetAnyTemplate_WhenGivenLanguageDoesNotExist_ShouldFallbackToEnglish()
        {
            // Arrange
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            var token = StringGenerator.GenerateUsingAsciiChars(10);
            var expirationTimeInHours = Random.Shared.Next();

            // Act
            var result = _emailTemplates.GetEmailVerificationTemplate(token, expirationTimeInHours);

            // Assert
            Assert.NotNull(result.Subject);
            Assert.Equal("Verify Your Email", result.Subject);
            Assert.NotNull(result.Body);

            var body = $"Please click the link below to verify your email.\n\n" +
                $"{_templateLinksSettings.EmailVerification}{token}\n\n" +
                $"The link is valid for {expirationTimeInHours} hours.";
            Assert.Equal(body, result.Body.Replace("\r\n", "\n"));
        }
    }
}
