using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Changes;
using Raven.Client.Connection;
using Raven.Client.Connection.Async;
using Raven.Client.Connection.Profiling;
using System.Reflection;
using System.Net;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using Raven.Client.Shard;
using RavenLinqpadDriver.Common;

namespace RavenLinqpadDriver
{
    public class RavenContext : IDocumentSession, IDocumentStore
    {
        private readonly IDocumentStore _docStore;
        private readonly Lazy<IDocumentSession> _lazySession;

        internal TextWriter LogWriter { get; set; }

        public RavenContext(RavenConnectionDialogViewModel connInfo)
        {
            if (connInfo == null) throw new ArgumentNullException("connInfo");

            _docStore = CreateDocStore(connInfo).Initialize();
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

            var entry = new StringBuilder().AppendFormat(@"
{0} - {1}
Url: {2}
Duration: {3} milliseconds
Method: {4}
Posted Data: {5}
Http Result: {6}
Result Data: {7}
Total Size: {8:n0}",
                e.At,
                e.Status,
                e.Url,
                e.DurationMilliseconds,
                e.Method,
                e.PostedData,
                e.HttpResult,
                e.Result,
                e.TotalSize);

            foreach (var item in e.AdditionalInformation)
                entry.AppendFormat("{0}: {1}", item.Key, item.Value);

            entry.AppendLine();
            LogWriter.WriteLine(entry.ToString());
        }

        private static IDocumentStore CreateDocStore(RavenConnectionDialogViewModel conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "conn is null.");

            var assemblies = conn.AssemblyPaths.Select(Path.GetFileNameWithoutExtension).Select(Assembly.Load);

            var docStoreCreatorType = (from a in assemblies
                                       from t in a.TypesImplementing<ICreateDocumentStore>()
                                       let hasDefaultCtor = t.GetConstructor(Type.EmptyTypes) != null
                                       where !t.IsAbstract && hasDefaultCtor
                                       select t).FirstOrDefault();

            if (docStoreCreatorType == null)
                return conn.CreateDocStore();

            var docStoreCreator = (ICreateDocumentStore)Activator.CreateInstance(docStoreCreatorType);

            var connectionInfo = new ConnectionInfo
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
            };

            return docStoreCreator.CreateDocumentStore(connectionInfo);
        }

        public void Dispose()
        {
            if (_lazySession.IsValueCreated)
                _lazySession.Value.Dispose();

            if (_docStore != null && !_docStore.WasDisposed)
                _docStore.Dispose();
        }


        public bool WasDisposed
        {
            get { return _docStore.WasDisposed; }
        }

        public event EventHandler AfterDispose;
        public IDatabaseChanges Changes(string database = null)
        {
            return _docStore.Changes(database);
        }

        public IDisposable AggressivelyCacheFor(TimeSpan cacheDuration)
        {
            return _docStore.AggressivelyCacheFor(cacheDuration);
        }

        public IDisposable AggressivelyCache()
        {
            return _docStore.AggressivelyCache();
        }

        public IDisposable DisableAggressiveCaching()
        {
            return _docStore.DisableAggressiveCaching();
        }

        public IDisposable SetRequestsTimeoutFor(TimeSpan timeout)
        {
            return _docStore.SetRequestsTimeoutFor(timeout);
        }

        public IDocumentStore Initialize()
        {
            return _docStore.Initialize();
        }

        public IAsyncDocumentSession OpenAsyncSession()
        {
            return _docStore.OpenAsyncSession();
        }

        public IAsyncDocumentSession OpenAsyncSession(string database)
        {
            return _docStore.OpenAsyncSession(database);
        }

        public IAsyncDocumentSession OpenAsyncSession(OpenSessionOptions sessionOptions)
        {
            return _docStore.OpenAsyncSession(sessionOptions);
        }


        public IDocumentSession OpenSession()
        {
            return _docStore.OpenSession();
        }

        public IDocumentSession OpenSession(string database)
        {
            return _docStore.OpenSession(database);
        }

        public IDocumentSession OpenSession(OpenSessionOptions sessionOptions)
        {
            return _docStore.OpenSession(sessionOptions);
        }

        public void ExecuteIndex(AbstractIndexCreationTask indexCreationTask)
        {
            _docStore.ExecuteIndex(indexCreationTask);
        }

        public Task ExecuteIndexAsync(AbstractIndexCreationTask indexCreationTask)
        {
            return _docStore.ExecuteIndexAsync(indexCreationTask);
        }

        public void ExecuteTransformer(AbstractTransformerCreationTask transformerCreationTask)
        {
            _docStore.ExecuteTransformer(transformerCreationTask);
        }

        public Task ExecuteTransformerAsync(AbstractTransformerCreationTask transformerCreationTask)
        {
            return _docStore.ExecuteTransformerAsync(transformerCreationTask);
        }

        public Etag GetLastWrittenEtag()
        {
            return _docStore.GetLastWrittenEtag();
        }

        public BulkInsertOperation BulkInsert(string database = null, BulkInsertOptions options = null)
        {
            return _docStore.BulkInsert(database, options);
        }

        public void SetListeners(DocumentSessionListeners listeners)
        {
            _docStore.SetListeners(listeners);
        }

        public void InitializeProfiling()
        {
            _docStore.InitializeProfiling();
        }

        public ProfilingInformation GetProfilingInformationFor(Guid id)
        {
            return _docStore.GetProfilingInformationFor(id);
        }

        public NameValueCollection SharedOperationsHeaders
        {
            get { return _docStore.SharedOperationsHeaders; }
        }

        public HttpJsonRequestFactory JsonRequestFactory
        {
            get { return _docStore.JsonRequestFactory; }
        }

        public bool HasJsonRequestFactory
        {
            get { return _docStore.HasJsonRequestFactory; }
        }

        public string Identifier
        {
            get { return _docStore.Identifier; }
            set { _docStore.Identifier = value; }
        }

        public IAsyncDatabaseCommands AsyncDatabaseCommands
        {
            get { return _docStore.AsyncDatabaseCommands; }
        }

        public IDatabaseCommands DatabaseCommands
        {
            get { return _docStore.DatabaseCommands; }
        }

        public DocumentConvention Conventions
        {
            get { return _docStore.Conventions; }
        }

        public string Url
        {
            get { return _docStore.Url; }
        }

        public IAsyncReliableSubscriptions AsyncSubscriptions
        {
            get { return _docStore.AsyncSubscriptions; }
        }

        public IReliableSubscriptions Subscriptions
        {
            get { return _docStore.Subscriptions; }
        }

        public DocumentSessionListeners Listeners
        {
            get { return _docStore.Listeners; }
        }

        public void Delete<T>(T entity)
        {
            _lazySession.Value.Delete(entity);
        }

        public void Delete<T>(ValueType id)
        {
            _lazySession.Value.Delete<T>(id);
        }

        public void Delete(string id)
        {
            _lazySession.Value.Delete(id);
        }

        public ILoaderWithInclude<object> Include(string path)
        {
            return _lazySession.Value.Include(path);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return _lazySession.Value.Include(path);
        }

        public ILoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, object>> path)
        {
            return _lazySession.Value.Include<T, TInclude>(path);
        }

        public T Load<T>(string id)
        {
            return _lazySession.Value.Load<T>(id);
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
            return _lazySession.Value.Load<T>(ids);
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

        public TResult Load<TTransformer, TResult>(string id, Action<ILoadConfiguration> configure = null) where TTransformer : AbstractTransformerCreationTask, new()
        {
            return _lazySession.Value.Load<TTransformer, TResult>(id, configure);
        }

        public TResult[] Load<TTransformer, TResult>(IEnumerable<string> ids, Action<ILoadConfiguration> configure = null) where TTransformer : AbstractTransformerCreationTask, new()
        {
            return _lazySession.Value.Load<TTransformer, TResult>(ids, configure);
        }

        public TResult Load<TResult>(string id, string transformer, Action<ILoadConfiguration> configure)
        {
            return _lazySession.Value.Load<TResult>(id, transformer, configure);
        }

        public TResult[] Load<TResult>(IEnumerable<string> ids, string transformer, Action<ILoadConfiguration> configure = null)
        {
            return _lazySession.Value.Load<TResult>(ids, transformer, configure);
        }

        public TResult Load<TResult>(string id, Type transformerType, Action<ILoadConfiguration> configure = null)
        {
            return _lazySession.Value.Load<TResult>(id, transformerType, configure);
        }

        public TResult[] Load<TResult>(IEnumerable<string> ids, Type transformerType, Action<ILoadConfiguration> configure = null)
        {
            return _lazySession.Value.Load<TResult>(ids, transformerType, configure);
        }

        public IRavenQueryable<T> Query<T>(string indexName, bool isMapReduce = false)
        {
            return _lazySession.Value.Query<T>(indexName, isMapReduce);
        }

        public IRavenQueryable<T> Query<T>()
        {
            return _lazySession.Value.Query<T>();
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return _lazySession.Value.Query<T, TIndexCreator>();
        }

        public void SaveChanges()
        {
            _lazySession.Value.SaveChanges();
        }

        public void Store(object entity, Etag etag)
        {
            _lazySession.Value.Store(entity, etag);
        }

        public void Store(object entity, Etag etag, string id)
        {
            _lazySession.Value.Store(entity, etag, id);
        }

        public void Store(object entity)
        {
            _lazySession.Value.Store(entity);
        }

        public void Store(object entity, string id)
        {
            _lazySession.Value.Store(entity, id);
        }

        public ISyncAdvancedSessionOperation Advanced
        {
            get { return _lazySession.Value.Advanced; }
        }

        public void SideBySideExecuteIndex(AbstractIndexCreationTask indexCreationTask, Etag minimumEtagBeforeReplace = null, DateTime? replaceTimeUtc = null)
        {
            _docStore.SideBySideExecuteIndex(indexCreationTask, minimumEtagBeforeReplace, replaceTimeUtc);
        }

        public Task SideBySideExecuteIndexAsync(AbstractIndexCreationTask indexCreationTask, Etag minimumEtagBeforeReplace = null, DateTime? replaceTimeUtc = null)
        {
            return _docStore.SideBySideExecuteIndexAsync(indexCreationTask, minimumEtagBeforeReplace, replaceTimeUtc);
        }

        public void SideBySideExecuteIndexes(List<AbstractIndexCreationTask> indexCreationTasks, Etag minimumEtagBeforeReplace = null, DateTime? replaceTimeUtc = default(DateTime?))
        {
            _docStore.SideBySideExecuteIndexes(indexCreationTasks, minimumEtagBeforeReplace, replaceTimeUtc);
        }

        public Task SideBySideExecuteIndexesAsync(List<AbstractIndexCreationTask> indexCreationTasks, Etag minimumEtagBeforeReplace = null, DateTime? replaceTimeUtc = default(DateTime?))
        {
            return _docStore.SideBySideExecuteIndexesAsync(indexCreationTasks, minimumEtagBeforeReplace, replaceTimeUtc);
        }

        public void ExecuteIndexes(List<AbstractIndexCreationTask> indexCreationTasks)
        {
            _docStore.ExecuteIndexes(indexCreationTasks);
        }

        public Task ExecuteIndexesAsync(List<AbstractIndexCreationTask> indexCreationTasks)
        {
            return _docStore.ExecuteIndexesAsync(indexCreationTasks);
        }
    }
}
