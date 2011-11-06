using System;
using System.Net;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;
using Raven.Client.Document;

namespace RavenLinqpadDriver
{
    public class RavenConnectionInfo
    {
        public const string RavenConnectionInfoKey = "RavenConnectionInfo";

        [JsonIgnore]
        public IConnectionInfo CxInfo { get; set; }

        public string Name { get; set; }

        private string _url = "http://localhost:8080";
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }
        public Guid? ResourceManagerId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DefaultDatabase { get; set; }
      
        /// <summary>
        /// trims string and returns null if empty; other wise returns trimmed string
        /// </summary>
        private string EmptyToNull(string source)
        {
            source = source.Trim();
            return source == "" ? null : source;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            CxInfo.DriverData.SetElementValue(RavenConnectionInfoKey, json);
        }

        public static RavenConnectionInfo Load(IConnectionInfo cxInfo)
        {
            XElement xe = cxInfo.DriverData.Element(RavenConnectionInfoKey);
            if (xe != null)
            {
                var json = xe.Value;
                var rvnConn = JsonConvert.DeserializeObject<RavenConnectionInfo>(json);
                rvnConn.CxInfo = cxInfo;
                return rvnConn;
            }

            return null;
        }

        public DocumentStore CreateDocStore()
        {
            try
            {
                var docStore = new DocumentStore
                {
                    Url = Url,
                    DefaultDatabase = DefaultDatabase
                };

                if (ResourceManagerId.HasValue)
                    docStore.ResourceManagerId = ResourceManagerId.Value;

                if (!string.IsNullOrEmpty(Username))
                {
                    docStore.Credentials = new NetworkCredential(Username, Password);
                }

                return docStore;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create DocumentStore", ex);
            }
        }
    }
}
