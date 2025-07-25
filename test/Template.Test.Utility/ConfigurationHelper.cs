using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Utility
{
    public static class ConfigurationHelper
    {
        private static readonly string _apiProjectPath = GetApiProjectPath();

        public static IConfiguration CreateConfiguration()
        {
            var apiProjectPath = GetApiProjectPath();

            return new ConfigurationBuilder()
                .AddUserSecrets<AppDbContextFixture>()
                .AddJsonFile(_apiProjectPath + "/appsettings.json")
                .Build();
        }

        public static ConfigurationManager CreateConfigurationManager()
        {
            var configManager = new ConfigurationManager();
            configManager.AddUserSecrets<AppDbContextFixture>();
            configManager.AddJsonFile(_apiProjectPath + "/appsettings.json");

            return configManager;
        }

        private static string GetApiProjectPath([CallerFilePath] string callerPath = "")
            => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(callerPath)!, @"..\..\src\Template.Api"));
    }
}
