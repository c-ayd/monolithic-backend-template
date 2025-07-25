using System.Net.Http.Headers;

namespace Template.Test.Utility.Fixtures.Hosting
{
    public partial class TestHostFixture
    {
        private List<ProductInfoHeaderValue>? defaultUserAgent = null;
        public void UpdateUserAgent(string name, string version)
        {
            defaultUserAgent = Client.DefaultRequestHeaders.UserAgent.ToList();
            Client.DefaultRequestHeaders.UserAgent.Clear();
            Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(name, version));
        }

        public void ResetUserAgent()
        {
            if (defaultUserAgent == null)
                return;

            Client.DefaultRequestHeaders.UserAgent.Clear();
            foreach (var item in defaultUserAgent)
            {
                Client.DefaultRequestHeaders.UserAgent.Add(item);
            }
        }
    }
}
