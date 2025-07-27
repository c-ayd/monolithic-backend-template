using System.Text;

namespace Template.Test.Utility
{
    public static class EmailHelper
    {
        public static readonly string _tempEmailPath = Path.GetFullPath(@".\Temp\Emails");

        public static async Task<EmailContent?> GetLatestTempEmailFile()
        {
            var emlFile = new DirectoryInfo(_tempEmailPath).GetFiles("*.eml")
                .OrderByDescending(f => f.CreationTime)
                .FirstOrDefault();

            if (emlFile == null)
                return null;

            await using var fileStream = emlFile.OpenRead();
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

        public static void ClearTempEmailFiles()
        {
            if (Directory.Exists(_tempEmailPath))
            {
                foreach (var file in Directory.GetFiles(_tempEmailPath))
                {
                    File.Delete(file);
                }
            }
        }

        public static void SetEmailSenderResult(bool success)
        {
            AppDomain.CurrentDomain.SetData("EmailSenderResult", success);
        }

        public class EmailContent
        {
            public string? SenderEmail { get; set; }
            public string? SenderDisplayName { get; set; }
            public string? ReceiverEmail { get; set; }
            public string? Subject { get; set; }
            public string? Body { get; set; }
            public EContentType ContentType { get; set; }

            public enum EContentType
            {
                None = 0,
                Plain = 1,
                Html = 2
            }
        }
    }
}
