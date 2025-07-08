using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Utility
{
    public static class ConfigurationHelper
    {
        public static IConfiguration CreateConfiguration()
        {
            var apiProjectPath = GetApiProjectPath();

            return new ConfigurationBuilder()
                .AddUserSecrets<AppDbContextFixture>()
                .AddJsonFile(apiProjectPath + "/appsettings.json")
                .Build();
        }

        private static string GetApiProjectPath([CallerFilePath] string callerPath = "")
            => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(callerPath)!, @"..\..\src\Template.Api"));
    }
}
