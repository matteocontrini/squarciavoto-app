using Newtonsoft.Json;
using Realms;
using System;
using System.Net;

namespace Gateway
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Message : RealmObject
    {
        [JsonProperty("sender")]
        public string Sender { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonIgnore]
        public string RequestStatusCode { get; set; }
    }
}