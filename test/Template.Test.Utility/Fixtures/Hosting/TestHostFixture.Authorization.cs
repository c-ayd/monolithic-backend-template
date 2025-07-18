using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using System.Net.Http.Headers;

namespace Template.Test.Utility.Fixtures.Hosting
{
    public partial class TestHostFixture
    {
        public void AddJwtBearerToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        }

        public void RemoveJwtBearerToken()
        {
            Client.DefaultRequestHeaders.Remove(HttpRequestHeader.Authorization.ToString());
        }
    }
}
