namespace PortScanner.Util
{
    using System;

    internal static class Utils
    {
        internal static bool IsDevelopmentEnvironment() =>
            string.Equals(
                Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);
    }
}
