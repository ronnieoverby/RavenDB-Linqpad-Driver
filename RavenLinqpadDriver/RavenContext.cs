using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Fasterflect;
using Raven.Client;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using RavenLinqpadDriver.Bridge;
using System.Reflection;
using System.Net;

namespace RavenLinqpadDriver
{
    public class RavenContext : IDocumentSession
    {
        private IDocumentStore _docStore;
        private IDocumentSession _session;
        private IDocumentSession Session
        {
            get
            {
                return _session ?? (_session = _docStore.OpenSession());
            }
        }
        internal TextWriter LogWriter { get; set; }

        public RavenContext(RavenConnectionDialogViewModel connInfo)
        {
            if (connInfo == null)
                throw new ArgumentNullException("conn", "conn is null.");

            InitDocStore(connInfo);
            SetupLogWriting();
        }


        private void SetupLogWriting()
        {
            _docStore.JsonRequestFactory.LogRequest += new EventHandler<RequestResultArgs>(LogRequest);
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

        private void InitDocStore(RavenConnectionDialogViewModel conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "conn is null.");

            var assemblies = new List<Assembly>(new[] 
            { 
                this.GetType().Assembly
            });

            foreach (var path in conn.GetAssemblyPaths())
                assemblies.Add(Assembly.LoadFrom(path));

            var docStoreCreatorType = (from a in assemblies
                                       from t in a.TypesImplementing<ICreateDocumentStore>()
                                       select t).FirstOrDefault();

            if (docStoreCreatorType != null)
            {
                var docStoreCreator = (ICreateDocumentStore)docStoreCreatorType.CreateInstance();
                _docStore = docStoreCreator.CreateDocumentStore(new ConnectionInfo
                {
                    Url = conn.Url,
                    DefaultDatabase = conn.DefaultDatabase,
                    Credentials = new NetworkCredential
                    {
                        UserName = conn.Username,
                        Password = conn.Password
                    },
                    ResourceManagerId = conn.ResourceManagerId,
                    //conn.ApiKey
                });
            }
            else
            {
                _docStore = conn.CreateDocStore();
            }

            _docStore.Initialize();
        }

        public void Dispose()
        {
            if (_session != null)
                _session.Dispose();

            if (_docStore != null && !_docStore.WasDisposed)
                _docStore.Dispose();
        }

        #region IDocumentSession Members
        public ISyncAdvancedSessionOperation Advanced
        {
            get { return Session.Advanced; }
        }

        public void Delete<T>(T entity)
        {
            Session.Delete<T>(entity);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return Session.Include<T>(path);
        }

        public ILoaderWithInclude<object> Include(string path)
        {
            return Session.Include(path);
        }

        public T Load<T>(ValueType id)
        {
            return Session.Load<T>(id);
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
            return Session.Load<T>(ids);
        }

        public T[] Load<T>(params string[] ids)
        {
            return Session.Load<T>(ids);
        }

        public T Load<T>(string id)
        {
            return Session.Load<T>(id);
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return Session.Query<T, TIndexCreator>();
        }

        public IRavenQueryable<T> Query<T>()
        {
            return Session.Query<T>();
        }

        public IRavenQueryable<T> Query<T>(string indexName)
        {
            return Session.Query<T>(indexName);
        }

        public void SaveChanges()
        {
            Session.SaveChanges();
        }

        public void Store(object entity, Guid etag, string id)
        {
            Session.Store(entity, etag, id);
        }

        public void Store(object entity, Guid etag)
        {
            Session.Store(entity, etag);
        }

#if !NET35
        public void Store(dynamic entity, string id)
        {
            Session.Store(entity, id);
        }

        public void Store(dynamic entity)
        {
            Session.Store(entity);
        }
#else
        public void Store(object entity, string id)
        {
            _session.Store(entity, id);
        }

        public void Store(object entity)
        {
            _session.Store(entity);
        }
#endif

        #endregion
    }
}
