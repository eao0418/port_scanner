namespace PortScanner.Converter
{
    using Newtonsoft.Json;
    using System;
    using System.Net;

    /* See https://stackoverflow.com/questions/18668617/json-net-error-getting-value-from-scopeid-on-system-net-ipaddress */

    /// <summary>
    /// A converter for <see cref="IPAddress"/>.
    /// </summary>
    class IPAddressConverter : JsonConverter
    {
        /// <summary>
        /// Writes the <see cref="IPAddress"/> to JSON.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/>.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue(value.ToString());

        /// <summary>
        /// Reads the <see cref="IPAddress"/> from JSON.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/>.</param>
        /// <param name="objectType">The <see cref="Type"/> of the object.</param>
        /// <param name="existingValue">The object.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/>.</param>
        /// <returns>The <see cref="IPAddress"/> parsed from the value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return IPAddress.Parse((string)reader.Value);
        }

        public override bool CanConvert(Type objectType) => (objectType == typeof(IPAddress));
    }
}
