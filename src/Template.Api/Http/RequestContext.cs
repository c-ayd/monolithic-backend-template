using System.Net;
using Template.Application.Abstractions.Http;

namespace Template.Api.Http
{
    public class RequestContext : IRequestContext
    {
        public Guid? UserId { get; set; }
        public IPAddress? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
        public List<string> PreferredLanguages { get; set; } = IRequestContext.DefaultPreferredLanguages;
    }
}
