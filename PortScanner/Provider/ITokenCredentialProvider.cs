using Azure.Core;

namespace PortScanner.Provider
{
    public interface ITokenCredentialProvider
    {
        TokenCredential GetTokenCredential();
    }
}