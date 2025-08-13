using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection;
using Template.Api.Settings;

namespace Template.Api.Configurations
{
    public static partial class Configurations
    {
        public static void ConfigureDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProtectionSettings = configuration.GetSection(DataProtectionSettings.SettingsKey).Get<DataProtectionSettings>()!;

            // NOTE: Consider changing the data protection approach depending on your need. Based on that, 'DataProtectionSettings'
            // might need to be changed as well.
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionSettings.FilePath))
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                })
                .SetApplicationName("monolithic-backend-template");
        }
    }
}
