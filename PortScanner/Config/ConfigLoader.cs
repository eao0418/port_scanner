namespace PortScanner.Config
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;

    /// <summary>
    /// A class to load the configuration for the application.
    /// </summary>
    internal static class ConfigLoader
    {
        /// <summary>
        /// Loads the configuration for the application.
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder"/> with the app settings for the environment.</returns>
        internal static IConfigurationBuilder LoadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Config"));

            if (string.Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddJsonFile("dev.settings.json", optional: false, reloadOnChange: true);
            }
            else
            {
                builder.AddJsonFile("prod.settings.json", optional: false, reloadOnChange: true);
            }

            return builder;
        }
    }
}
