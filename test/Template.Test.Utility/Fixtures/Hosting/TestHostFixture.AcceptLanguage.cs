using System.Net.Http.Headers;

namespace Template.Test.Utility.Fixtures.Hosting
{
    public partial class TestHostFixture
    {
        private List<StringWithQualityHeaderValue>? defaultAcceptLanguage = null;
        public void UpdateAcceptLanguage(string culture)
        {
            defaultAcceptLanguage = Client.DefaultRequestHeaders.AcceptLanguage.ToList();
            Client.DefaultRequestHeaders.AcceptLanguage.Clear();
            Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        }

        public void ResetAcceptLanguage()
        {
            if (defaultAcceptLanguage == null)
                return;

            Client.DefaultRequestHeaders.AcceptLanguage.Clear();
            foreach (var item in defaultAcceptLanguage)
            {
                Client.DefaultRequestHeaders.AcceptLanguage.Add(item);
            }
        }
    }
}
