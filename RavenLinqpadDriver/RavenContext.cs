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
    public class RavenContext : IDocumentSession, IDocumentStore
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
                    ApiKey = conn.ApiKey
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

        #region IDocumentStore Members

#if !NET35
        public Raven.Client.Connection.Async.IAsyncDatabaseCommands AsyncDatabaseCommands
        {
            get { return _docStore.AsyncDatabaseCommands; }
        }

        public IAsyncDocumentSession OpenAsyncSession(string database)
        {
            return _docStore.OpenAsyncSession(database);
        }

        public IAsyncDocumentSession OpenAsyncSession()
        {
            return _docStore.OpenAsyncSession();
        }
#endif

        public IDisposable AggressivelyCacheFor(TimeSpan cahceDuration)
        {
            return _docStore.AggressivelyCacheFor(cahceDuration);
        }

        public DocumentConvention Conventions
        {
            get { return _docStore.Conventions; }
        }

        public Raven.Client.Connection.IDatabaseCommands DatabaseCommands
        {
            get { return _docStore.DatabaseCommands; }
        }

        public IDisposable DisableAggressiveCaching()
        {
            return  _docStore.DisableAggressiveCaching();
        }

        public void ExecuteIndex(AbstractIndexCreationTask indexCreationTask)
        {
            _docStore.ExecuteIndex(indexCreationTask);
        }

        public Guid? GetLastWrittenEtag()
        {
            return _docStore.GetLastWrittenEtag();
        }

        public string Identifier
        {
            get
            {
                return _docStore.Identifier;
            }
            set
            {
                _docStore.Identifier = value;
            }
        }

        public IDocumentStore Initialize()
        {
            // already initialized
            return this;
        }

        public Raven.Client.Connection.HttpJsonRequestFactory JsonRequestFactory
        {
            get { return _docStore.JsonRequestFactory; }
        }

        public IDocumentSession OpenSession(OpenSessionOptions sessionOptions)
        {
            return _docStore.OpenSession(sessionOptions);
        }

        public IDocumentSession OpenSession(string database)
        {
            return _docStore.OpenSession(database);
        }

        public IDocumentSession OpenSession()
        {
            return _docStore.OpenSession();
        }

        public System.Collections.Specialized.NameValueCollection SharedOperationsHeaders
        {
            get { return _docStore.SharedOperationsHeaders; }
        }

        public string Url
        {
            get { return _docStore.Url; }
        }

        public event EventHandler AfterDispose;

        public bool WasDisposed
        {
            get { return _docStore.WasDisposed; }
        } 

        #endregion
    }
}
