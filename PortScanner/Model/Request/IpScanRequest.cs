namespace PortScanner.Model.Request
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using PortScanner.Converter;
    using System.Net;

    public class IpScanRequest
    {
        [JsonConverter(typeof(IPAddressConverter))]
        public IPAddress IPAddress { get; set; }

        public string ScanId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Site Site { get; set; }
    }
}
