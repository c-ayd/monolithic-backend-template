using System.Net.Http.Headers;

namespace Template.Test.Utility.Fixtures.Hosting
{
    public partial class TestHostFixture
    {
        private List<StringWithQualityHeaderValue>? defaultAcceptLanguage = null;
        public void UpdateAcceptLanguage(IDictionary<string, double> languagesWithQualities)
        {
            defaultAcceptLanguage = Client.DefaultRequestHeaders.AcceptLanguage.ToList();
            Client.DefaultRequestHeaders.AcceptLanguage.Clear();

            foreach (var language in languagesWithQualities)
            {
                Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(language.Key, language.Value));
            }
        }

        public void UpdateAcceptLanguage(string language)
        {
            defaultAcceptLanguage = Client.DefaultRequestHeaders.AcceptLanguage.ToList();
            Client.DefaultRequestHeaders.AcceptLanguage.Clear();
            Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));
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
