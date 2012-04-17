using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RavenLinqpadDriver.Bridge
{
    public class ConnectionInfo
    {
        public string Url { get; set; }
        public string DefaultDatabase { get; set; }
        public NetworkCredential Credentials { get; set; }
        public Guid? ResourceManagerId { get; set; }

        
    }
}
