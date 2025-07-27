using Microsoft.Net.Http.Headers;

namespace Template.Test.Utility.Fixtures.Hosting
{
    public partial class TestHostFixture
    {
        public void SetCookies(IDictionary<string, string> cookies)
        {
            var cookieHeaderValue = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));
            Client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookieHeaderValue);
        }

        public void ClearCookies()
        {
            Client.DefaultRequestHeaders.Remove(HeaderNames.Cookie);
        }
    }
}
