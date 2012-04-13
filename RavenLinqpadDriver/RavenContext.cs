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

namespace RavenLinqpadDriver
{
    public class RavenContext : IDocumentSession
    {
        private IDocumentStore _docStore;
        private IDocumentSession _session;
        internal TextWriter LogWriter { get; set; }

        public RavenContext(RavenConnectionInfo connInfo)
        {
            if (connInfo == null)
                throw new ArgumentNullException("conn", "conn is null.");

            InitDocStore(connInfo);
            SetupLogWriting();
            _session = _docStore.OpenSession();
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

        private void InitDocStore(RavenConnectionInfo conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "conn is null.");

            _docStore = conn.CreateDocStore();

            // search for a user defined initializer
            var refAssemblyNames = this.GetType().Assembly.GetReferencedAssemblies();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var initType = (from a in assemblies
                            //let a = Assembly.Load(an)
                            from t in a.TypesImplementing<IConfigureDocumentStore>()
                            select t).FirstOrDefault();

            if (initType != null)
            {
                var docStoreInit = (IConfigureDocumentStore)initType.CreateInstance();
                docStoreInit.ConfigureDocumentStore(_docStore);
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
            get { return _session.Advanced; }
        }

        public void Delete<T>(T entity)
        {
            _session.Delete<T>(entity);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return _session.Include<T>(path);
        }

        public ILoaderWithInclude<object> Include(string path)
        {
            return _session.Include(path);
        }

        public T Load<T>(ValueType id)
        {
            return _session.Load<T>(id);
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
            return _session.Load<T>(ids);
        }

        public T[] Load<T>(params string[] ids)
        {
            return _session.Load<T>(ids);
        }

        public T Load<T>(string id)
        {
            return _session.Load<T>(id);
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return _session.Query<T, TIndexCreator>();
        }

        public IRavenQueryable<T> Query<T>()
        {
            return _session.Query<T>();
        }

        public IRavenQueryable<T> Query<T>(string indexName)
        {
            return _session.Query<T>(indexName);
        }

        public void SaveChanges()
        {
            _session.SaveChanges();
        }

        public void Store(object entity, Guid etag, string id)
        {
            _session.Store(entity, etag, id);
        }

        public void Store(object entity, Guid etag)
        {
            _session.Store(entity, etag);
        }

#if !NET35
        public void Store(dynamic entity, string id)
        {
            _session.Store(entity, id);
        }

        public void Store(dynamic entity)
        {
            _session.Store(entity);
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
