namespace Template.Application.Abstractions.Messaging
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body, bool isBodyHtml = true);
    }
}
