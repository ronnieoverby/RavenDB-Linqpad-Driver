using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Document;
using Raven.Client;
using System.IO;
using Raven.Client.Connection.Profiling;
using Raven.Client.Connection;
using System.Reflection;

namespace RavenLinqpadDriver
{
    public class RavenContext : IDisposable
    {
        private DocumentStore _docStore;
        public DocumentStore DocStore
        {
            get
            {
                return _docStore;
            }
            set
            {
                if (_docStore != null && !_docStore.WasDisposed)
                    _docStore.Dispose();

                _docStore = value;

                if (_docStore != null)
                    _docStore.Initialize();
            }
        }
        public IDocumentSession Session { get; private set; }
        internal TextWriter LogWriter { get; set; }

        public RavenContext(RavenConnectionInfo connInfo)
        {
            if (connInfo == null)
                throw new ArgumentNullException("conn", "conn is null.");

            InitDocStore(connInfo);
            SetupLogWriting();
            InitSession();
        }

        private void SetupLogWriting()
        {
            DocStore.JsonRequestFactory.LogRequest += new EventHandler<RequestResultArgs>(LogRequest);
        }       

        void LogRequest(object sender, RequestResultArgs e)
        {
            if (LogWriter == null) return;

            LogWriter.WriteLine(string.Format(@"
{0} - {1}
Url: {2}
Duration: {3} milliseconds
Method: {4}
Posted Data: {5}
Http Result: {6}
Result Data: {7}
",
                e.At, // 0
                e.Status, // 1
                e.Url, // 2
                e.DurationMilliseconds, // 3
                e.Method, // 4
                e.PostedData, // 5
                e.HttpResult, // 6
                e.Result)); // 7
        }

        private void InitDocStore(RavenConnectionInfo conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "conn is null.");

            DocStore = conn.CreateDocStore();
            DocStore.Initialize();            
        }

        private void InitSession()
        {
            Session = DocStore.OpenSession();
        }

        public void Dispose()
        {
            if (Session != null)
                Session.Dispose();

            if (DocStore != null && !DocStore.WasDisposed)
                DocStore.Dispose();
        }
    }
}
