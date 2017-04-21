using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventGateway.Core
{
    public class Message
    {
        public string Label { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public JObject Payload { get; set; }

        public override string ToString()
        {
            return $"Label={Label}";
        }
    }
}
