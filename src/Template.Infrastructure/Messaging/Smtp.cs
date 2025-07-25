using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Template.Application.Abstractions.Messaging;
using Template.Infrastructure.Settings.Messaging;

namespace Template.Infrastructure.Messaging
{
    public class Smtp : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public Smtp(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendAsync(string to, string subject, string body, bool isBodyHtml = true)
        {
            var email = new MailMessage()
            {
                From = new MailAddress(_smtpSettings.Email, _smtpSettings.DisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };
            email.To.Add(to);

            using var smtpClient = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port);
#if DEBUG
            var isEmailSent = (bool?)AppDomain.CurrentDomain.GetData("EmailSenderResult");
            if (isEmailSent.HasValue && !isEmailSent.Value)
                throw new Exception("The email is not sent for testing.");

            var fullPath = Path.GetFullPath(@".\Temp\Emails");
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            smtpClient.PickupDirectoryLocation = fullPath;
#else
            smtpClient.Credentials = new NetworkCredential(_smtpSettings.Email, _smtpSettings.Password);
            smtpClient.EnableSsl = true;
#endif

            await smtpClient.SendMailAsync(email);
        }
    }
}
