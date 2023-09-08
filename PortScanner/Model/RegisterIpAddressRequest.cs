namespace PortScanner.Model
{
    using System.Text.Json.Serialization;

    public class RegisterIpAddressRequest
    {
        /// <summary>
        /// The first IP address in the range to scan
        /// </summary>
        public string StartIpAddress { get; set; }

        /// <summary>
        /// Represents the CIDR range of the ip address.
        /// </summary>
        public int CidrRange { get; set; }

        /// <summary>
        /// The site the IP address is allocated in.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Site Site { get; set; }
    }
}
