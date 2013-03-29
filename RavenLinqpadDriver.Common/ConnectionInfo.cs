using System;
using System.Net;

namespace RavenLinqpadDriver.Common
{
    public class ConnectionInfo
    {
        public string Url { get; set; }
        public string DefaultDatabase { get; set; }
        public NetworkCredential Credentials { get; set; }
        public Guid? ResourceManagerId { get; set; }
        public string ApiKey { get; set; }
    }
}
