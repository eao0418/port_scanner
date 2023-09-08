namespace PortScanner.Model.TableEntity
{
    using Azure.Data.Tables;
    using PortScanner.Converter;
    using System;
    using System.Net;
    using System.Text.Json.Serialization;

    public class ScanIpEntity
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Site Site { get; set; }
        [JsonConverter(typeof(IPAddressConverter))]
        public IPAddress IPAddress { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ScanIpEntity"/>.
        /// </summary>
        public ScanIpEntity() { }

        /// <summary>
        /// Creates a new <see cref="ITableEntity"/> from the <see cref="ScanIpEntity"/>.
        /// </summary>
        /// <returns>A <see cref="ITableEntity"/>.</returns>
        public ITableEntity ToTableEntity()
        {
            return new TableEntity()
            {
                PartitionKey = Site.ToString(),
                RowKey = IPAddress.ToString(),
                Timestamp = CreatedOn,
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="ScanIpEntity"/>.
        /// </summary>
        /// <param name="site">The <see cref="Site"/>.</param>
        /// <param name="ipaddress">The <see cref="IPAddress"/>.</param>
        /// <returns>A <see cref="ScanIpEntity"/>.</returns>
        public static ScanIpEntity Create(Site site, IPAddress ipaddress)
        {
            return new ScanIpEntity()
            {
                Site = site,
                IPAddress = ipaddress,
                CreatedOn = DateTimeOffset.UtcNow,
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="ScanIpEntity"/> from a <see cref="ITableEntity"/>.
        /// </summary>
        /// <param name="tableEntity">The <see cref="ITableEntity"/> to convert.</param>
        /// <returns>A <see cref="ScanIpEntity"/>.</returns>
        public static ScanIpEntity FromTableEntity(ITableEntity tableEntity)
        {
            return new ScanIpEntity()
            {
                Site = Enum.Parse<Site>(tableEntity.PartitionKey),
                IPAddress = IPAddress.Parse(tableEntity.RowKey),
                CreatedOn = tableEntity.Timestamp,
            };
        }
    }
}
