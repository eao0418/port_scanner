namespace PortScanner.Provider
{
    using Azure.Core;
    using Azure.Identity;

    public class TokenCredentialProvider : ITokenCredentialProvider
    {
        public TokenCredentialProvider() { }

        public TokenCredential GetTokenCredential()
        {
            return new DefaultAzureCredential();
        }
    }
}
