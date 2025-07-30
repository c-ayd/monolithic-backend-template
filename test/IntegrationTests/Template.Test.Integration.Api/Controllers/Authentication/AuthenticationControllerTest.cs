using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions.Authentication;
using Template.Application.Abstractions.Crypto;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Controllers.Authentication
{
    [Collection(nameof(TestHostCollection))]
    public partial class AuthenticationControllerTest : IDisposable
    {
        private readonly TestHostFixture _testHostFixture;
        private readonly IHashing _hashing;
        private readonly IJwt _jwt;

        public AuthenticationControllerTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
            _testHostFixture.SetDefaultOptions();

            _hashing = new Hashing();

            var config = ConfigurationHelper.CreateConfiguration();
            var jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;
            _jwt = new Jwt(Options.Create(jwtSettings), new TokenGenerator());
        }

        public void Dispose()
        {
            EmailHelper.ClearTempEmailFiles();
        }
    }
}
