using Template.Application.Abstractions.Crypto;
using Template.Infrastructure.Crypto;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Controllers
{
    [Collection(nameof(TestHostCollection))]
    public partial class AuthenticationControllerTest
    {
        private readonly TestHostFixture _testHostFixture;
        private readonly IHashing _hashing;

        public AuthenticationControllerTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;

            _hashing = new Hashing();
        }
    }
}
