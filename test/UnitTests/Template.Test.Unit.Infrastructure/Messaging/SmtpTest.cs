using Cayd.Test.Generators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Template.Infrastructure.Messaging;
using Template.Infrastructure.Settings.Messaging;
using Template.Test.Utility;

namespace Template.Test.Unit.Infrastructure.Messaging
{
    public class SmtpTest : IDisposable
    {
        private readonly string _senderEmail = "test@test.com";
        private readonly string _senderDisplayName = "Test Test";

        private readonly Smtp _smtp;

        public SmtpTest()
        {
            var config = ConfigurationHelper.CreateConfiguration();
            var smtpSettings = config.GetSection(SmtpSettings.SettingsKey).Get<SmtpSettings>()!;

            smtpSettings.Email = _senderEmail;
            smtpSettings.DisplayName = _senderDisplayName;

            _smtp = new Smtp(Options.Create(smtpSettings));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendAsync_WhenEmailAddressAndCredentialsAreCorrect_ShouldSendEmail(bool isBodyHtml)
        {
            // Arrange
            var to = EmailGenerator.Generate();
            var subject = "Welcome";
            var body = "Hello User!";

            // Act
            await _smtp.SendAsync(to, subject, body, isBodyHtml);

            // Assert
            var emailContent = await EmailHelper.GetLatestTempEmailFileAsync();

            Assert.NotNull(emailContent);
            Assert.NotNull(emailContent.SenderDisplayName);
            Assert.Equal(_senderDisplayName, emailContent.SenderDisplayName);
            Assert.NotNull(emailContent.SenderEmail);
            Assert.Equal(_senderEmail, emailContent.SenderEmail);
            Assert.NotNull(emailContent.ReceiverEmail);
            Assert.Equal(to, emailContent.ReceiverEmail);
            Assert.NotNull(emailContent.Subject);
            Assert.Equal(subject, emailContent.Subject);
            Assert.NotNull(emailContent.Body);
            Assert.Equal(body.TrimEnd('\r', '\n'), emailContent.Body);
            Assert.Equal(isBodyHtml ? EmailHelper.EmailContent.EContentType.Html : EmailHelper.EmailContent.EContentType.Plain, emailContent.ContentType);
        }

        [Fact]
        public async Task SendAsync_WhenEmailAddressIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            var result = await Record.ExceptionAsync(async () =>
            {
                await _smtp.SendAsync(null, "test", "test");
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentNullException>(result);
        }

        [Fact]
        public async Task SendAsync_WhenEmailAddressIsEmpty_ShouldThrowArgumentException()
        {
            // Act
            var result = await Record.ExceptionAsync(async () =>
            {
                await _smtp.SendAsync("", "test", "test");
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArgumentException>(result);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("abc")]
        public async Task SendAsync_WhenEmailAddressIsInvalid_ShouldThrowFormatException(string? to)
        {
            // Act
            var result = await Record.ExceptionAsync(async () =>
            {
                await _smtp.SendAsync(to, "test", "test");
            });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormatException>(result);
        }

        public void Dispose()
        {
            EmailHelper.ClearTempEmailFiles();
        }
    }
}
