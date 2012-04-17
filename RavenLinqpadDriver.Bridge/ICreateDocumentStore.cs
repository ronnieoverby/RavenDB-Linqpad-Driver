using Raven.Client;

namespace RavenLinqpadDriver.Bridge
{
    public interface ICreateDocumentStore
    {
        /// <summary>
        /// Creates an IDocumentStore.
        /// </summary>
        /// <param name="connectionInfo">The connection information for a RavenDB database.</param>
        /// <returns>An IDocumentStore.</returns>
        IDocumentStore CreateDocumentStore(ConnectionInfo connectionInfo);
    }
}
