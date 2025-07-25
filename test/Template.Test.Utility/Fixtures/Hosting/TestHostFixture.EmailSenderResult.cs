namespace Template.Test.Utility.Fixtures.Hosting
{
    public partial class TestHostFixture
    {
        public void SetEmailSenderResult(bool result)
        {
            AppDomain.CurrentDomain.SetData("EmailSenderResult", result);
        }
    }
}
