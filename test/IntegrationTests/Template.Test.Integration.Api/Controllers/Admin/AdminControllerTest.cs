using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions.Authentication;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Controllers.Admin
{
    [Collection(nameof(TestHostCollection))]
    public partial class AdminControllerTest
    {
        private readonly TestHostFixture _testHostFixture;
        private readonly IJwt _jwt;

        public AdminControllerTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
            _testHostFixture.SetDefaultOptions();

            var config = ConfigurationHelper.CreateConfiguration();
            var jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;
            _jwt = new Jwt(Options.Create(jwtSettings), new TokenGenerator());
        }
    }
}
