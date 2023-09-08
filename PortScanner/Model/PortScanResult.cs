namespace PortScanner.Model
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using PortScanner.Converter;
    using System;
    using System.Net;

    public class PortScanResult
    {
        public int Port { get; set; }

        public bool IsOpen { get; set; }

        public string ScanId { get; set; }

        [JsonConverter(typeof(IPAddressConverter))]
        public IPAddress IPAddress { get; set; }

        public DateTime ScanDate { get; set; }

        public string Protocol { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Site Site { get; set; }
    }
}
