using Realms;
using System;
using System.Net;

namespace Gateway
{
    public class Message : RealmObject
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Date { get; set; }
        public string RequestStatusCode { get; set; }
    }
}