using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api
{
    [Collection(nameof(TestHostCollection))]
    public partial class AuthTest
    {
        private readonly TestHostFixture _testHostFixture;
        private readonly JwtSettings _jwtSettings;
        private readonly Jwt _jwt;

        public AuthTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;

            var config = ConfigurationHelper.CreateConfiguration();
            _jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;

            _jwt = new Jwt(Options.Create(_jwtSettings), new TokenGenerator());
        }
    }
}
