namespace PortScanner.Model
{
    using System.Text.Json.Serialization;

    public class SiteScanRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Site Site { get; set; }

        public string ScanId { get; set; }
    }
}
