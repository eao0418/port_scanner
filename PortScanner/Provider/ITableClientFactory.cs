namespace PortScanner.Provider
{
    using Azure.Data.Tables;
    using System.Threading.Tasks;

    public interface ITableClientFactory
    {
        /// <summary>
        /// Gets a <see cref="TableClient"/> for the given table.
        /// </summary>
        /// <param name="tableName">The table to get the client for.</param>
        /// <returns>A <see cref="Task{TableClient}"/></returns>
        public Task<TableClient> GetTableClient(string tableName);
    }
}
