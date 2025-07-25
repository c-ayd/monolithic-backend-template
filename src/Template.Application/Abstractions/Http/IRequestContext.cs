using System.Net;

namespace Template.Application.Abstractions.Http
{
    public interface IRequestContext
    {
        public static List<string> DefaultPreferredLanguages = new List<string>() { "en" };

        Guid? UserId { get; }
        IPAddress? IpAddress { get; }
        string? DeviceInfo { get; }
        List<string> PreferredLanguages { get; }
    }
}
