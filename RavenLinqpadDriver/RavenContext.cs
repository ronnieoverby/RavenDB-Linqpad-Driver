using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Fasterflect;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Changes;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using System.Reflection;
using System.Net;
using Raven.Client.Shard;
using RavenLinqpadDriver.Common;

namespace RavenLinqpadDriver
{
    public class RavenContext : IDocumentSession, IDocumentStore
    {
        private IDocumentStore _docStore;
        private readonly Lazy<IDocumentSession> _lazySession;

        internal TextWriter LogWriter { get; set; }

        public RavenContext(RavenConnectionDialogViewModel connInfo)
        {
            if (connInfo == null) throw new ArgumentNullException("connInfo");

            InitDocStore(connInfo);
            _lazySession = new Lazy<IDocumentSession>(_docStore.OpenSession);
            SetupLogWriting();
        }

        private void SetupLogWriting()
        {
            // sharded doc stores don't have a jsonrequestfactory,
            // so if sharding, get shard doc stores
            var shardedDocStore = _docStore as ShardedDocumentStore;
            if (shardedDocStore != null)
            {
                var docStores = from ds in shardedDocStore.ShardStrategy.Shards.Values
                                where !(ds is ShardedDocumentStore)
                                select ds;

                foreach (var docStore in docStores)
                    docStore.JsonRequestFactory.LogRequest += LogRequest;
            }
            else
            {
                _docStore.JsonRequestFactory.LogRequest += LogRequest;
            }
        }

        void LogRequest(object sender, RequestResultArgs e)
        {
            if (LogWriter == null) return;

            LogWriter.WriteLine(@"
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
                                e.Result); // 7
        }

        private void InitDocStore(RavenConnectionDialogViewModel conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "conn is null.");



            var assemblies = conn.GetAssemblyPaths().Select(Path.GetFileNameWithoutExtension).Select(Assembly.Load);

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
            if (_lazySession.IsValueCreated)
                _lazySession.Value.Dispose();

            if (_docStore != null && !_docStore.WasDisposed)
                _docStore.Dispose();
        }

        #region IDocumentSession Members
        public ISyncAdvancedSessionOperation Advanced
        {
            get { return _lazySession.Value.Advanced; }
        }

        public void Delete<T>(T entity)
        {
            _lazySession.Value.Delete(entity);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return _lazySession.Value.Include(path);
        }

        public ILoaderWithInclude<object> Include(string path)
        {
            return _lazySession.Value.Include(path);
        }

        public T Load<T>(ValueType id)
        {
            return _lazySession.Value.Load<T>(id);
        }

        public T[] Load<T>(params ValueType[] ids)
        {
            return _lazySession.Value.Load<T>(ids);
        }

        public T[] Load<T>(IEnumerable<ValueType> ids)
        {
            return _lazySession.Value.Load<T>(ids);
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        public IRavenQueryable<T> Query<T>(string indexName, bool isMapReduce = false)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            return _lazySession.Value.Query<T>(indexName, isMapReduce);
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
            return _lazySession.Value.Load<T>(ids);
        }

        public T[] Load<T>(params string[] ids)
        {
            return _lazySession.Value.Load<T>(ids);
        }

        public T Load<T>(string id)
        {
            return _lazySession.Value.Load<T>(id);
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return _lazySession.Value.Query<T, TIndexCreator>();
        }

        public IRavenQueryable<T> Query<T>()
        {
            return _lazySession.Value.Query<T>();
        }

        public IRavenQueryable<T> Query<T>(string indexName)
        {
            return _lazySession.Value.Query<T>(indexName);
        }

        public void SaveChanges()
        {
            _lazySession.Value.SaveChanges();
        }

        public void Store(object entity, Guid etag, string id)
        {
            _lazySession.Value.Store(entity, etag, id);
        }

        public void Store(object entity, Guid etag)
        {
            _lazySession.Value.Store(entity, etag);
        }


        public void Store(dynamic entity, string id)
        {
            _lazySession.Value.Store(entity, id);
        }

        public void Store(dynamic entity)
        {
            _lazySession.Value.Store(entity);
        }

        public ILoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, object>> path)
        {
            return _lazySession.Value.Include<T, TInclude>(path);
        }

        #endregion

        #region IDocumentStore Members

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

        public IDatabaseChanges Changes(string database = null)
        {
            return _docStore.Changes(database);
        }

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
            return _docStore.DisableAggressiveCaching();
        }

        public void ExecuteIndex(AbstractIndexCreationTask indexCreationTask)
        {
            _docStore.ExecuteIndex(indexCreationTask);
        }

        public Guid? GetLastWrittenEtag()
        {
            return _docStore.GetLastWrittenEtag();
        }

        public BulkInsertOperation BulkInsert(string database = null, BulkInsertOptions options = null)
        {
            return _docStore.BulkInsert(database, options);
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
