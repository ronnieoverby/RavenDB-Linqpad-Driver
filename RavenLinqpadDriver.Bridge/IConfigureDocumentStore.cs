using Raven.Client;

namespace RavenLinqpadDriver.Bridge
{
    public interface IConfigureDocumentStore
    {
        /// <summary>
        /// Completes configuration the DocumentStore.        
        /// </summary>
        /// <param name="preConfiguredDocStore">A DocumentStore object that has been configured with the values from the connection properties dialog window.</param>
        /// <returns>A completely configured DocumentStore instance.</returns>
        IDocumentStore ConfigureDocumentStore(IDocumentStore preConfiguredDocStore);
    }
}
