namespace PortScanner.Provider
{
    using System;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using Microsoft.Extensions.Options;
    using System.Collections.Concurrent;

    public class TableClientFactory: ITableClientFactory
    {
        private readonly TableServiceClient serviceClient;
        private readonly ConcurrentDictionary<string, TableClient> tableClients = new ConcurrentDictionary<string, TableClient>();

        public TableClientFactory(ITokenCredentialProvider tokenCredentialProvider, IOptions<PortScannerOptions> options) 
        {
            if (tokenCredentialProvider is null)
            {
                throw new ArgumentNullException(nameof(tokenCredentialProvider));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var portScannerOptions = options.Value;

            var credential = tokenCredentialProvider.GetTokenCredential();

            serviceClient = new TableServiceClient(new Uri(portScannerOptions.TableServiceAccountEndpoint), credential);
        }

        public TableClientFactory()
        {
            this.serviceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        }

        public async Task<TableClient> GetTableClient(string tableName)
        {
            var client = tableClients.GetOrAdd(tableName, _ =>
                serviceClient.GetTableClient(tableName)
            );

            await client.CreateIfNotExistsAsync();

            return client;
        }   
    }
}
