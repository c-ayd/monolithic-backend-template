using Cayd.Test.Generators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using Template.Infrastructure.Messaging;
using Template.Infrastructure.Settings.Messaging;
using Template.Test.Utility;

namespace Template.Test.Unit.Infrastructure.Messaging
{
    public class SmtpTest : IDisposable
    {
        private static readonly string _senderEmail = "test@test.com";
        private static readonly string _senderDisplayName = "Test Test";

        private readonly Smtp _smtp;

        private static readonly string _tempEmailPath = Path.GetFullPath(@".\Temp\Emails");

        public SmtpTest()
        {
            var config = ConfigurationHelper.CreateConfiguration();
            var smtpSettings = config.GetSection(SmtpSettings.SettingsKey).Get<SmtpSettings>()!;

            smtpSettings.Email = _senderEmail;
            smtpSettings.DisplayName = _senderDisplayName;

            _smtp = new Smtp(Options.Create(smtpSettings));
        }

        private void ClearTempEmailFiles()
        {
            if (Directory.Exists(_tempEmailPath))
            {
                foreach (var file in Directory.GetFiles(_tempEmailPath))
                {
                    File.Delete(file);
                }
            }
        }

        private async Task<EmailContent?> GetLatestTempEmailFile()
        {
            var emlFile = new DirectoryInfo(_tempEmailPath).GetFiles("*.eml")
                .OrderByDescending(f => f.CreationTime)
                .FirstOrDefault();

            if (emlFile == null)
                return null;

            using var fileStream = emlFile.OpenRead();
            using var streamReader = new StreamReader(fileStream);

            var emailContent = new EmailContent();

            StringBuilder builder = new StringBuilder();
            bool isReadingBody = false;
            while (true)
            {
                var line = await streamReader.ReadLineAsync();
                if (line == null)
                    break;

                if (isReadingBody)
                {
                    builder.AppendLine(line);
                }
                else
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        isReadingBody = true;
                    }
                    else if (line.StartsWith("From:"))
                    {
                        var from = line.Split(':')[1].Trim();
                        var emailStartIndex = from.IndexOf('<');
                        var emailEndIndex = from.IndexOf('>');

                        emailContent.SenderDisplayName = from.Substring(0, emailStartIndex).Trim().Trim('"');
                        emailContent.SenderEmail = from.Substring(emailStartIndex + 1, emailEndIndex - emailStartIndex - 1);
                    }
                    else if (line.StartsWith("To:"))
                    {
                        emailContent.ReceiverEmail = line.Split(':')[1].Trim();
                    }
                    else if (line.StartsWith("Subject:"))
                    {
                        emailContent.Subject = line.Split(':')[1].Trim();
                    }
                    else if (line.StartsWith("Content-Type:"))
                    {
                        var contentType = line.Split(':')[1].Split(';')[0].Trim();
                        if (contentType == "text/plain")
                        {
                            emailContent.ContentType = EmailContent.EContentType.Plain;
                        }
                        else if (contentType == "text/html")
                        {
                            emailContent.ContentType = EmailContent.EContentType.Html;
                        }
                    }
                }
            }

            emailContent.Body = builder.ToString().TrimEnd('\r', '\n');

            return emailContent;
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
            var emailContent = await GetLatestTempEmailFile();

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
            Assert.Equal(isBodyHtml ? EmailContent.EContentType.Html : EmailContent.EContentType.Plain, emailContent.ContentType);
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
            ClearTempEmailFiles();
        }

        private class EmailContent
        {
            public string? SenderEmail { get; set; }
            public string? SenderDisplayName { get; set; }
            public string? ReceiverEmail { get; set; }
            public string? Subject { get; set; }
            public string? Body { get; set; }
            public EContentType ContentType { get; set; }

            public enum EContentType
            {
                None        =   0,
                Plain       =   1,
                Html        =   2
            }
        }
    }
}
