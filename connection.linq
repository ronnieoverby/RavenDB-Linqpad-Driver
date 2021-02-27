<Query Kind="Program">
  <NuGetReference>RavenDB.Client</NuGetReference>
  <Namespace>Raven.Client.Documents</Namespace>
  <Namespace>Raven.Client.Documents.Session</Namespace>
</Query>

// just load this query from other queries
// this query serves as the linqpad "connection" for lack of an actual driver

IDocumentStore DocumentStore;
IDocumentSession Session;

// linqpad's Hijack method will replace (and invoke) the main method in the referencing query
void Hijack()
{
	static IDocumentStore CreateDocumentStore()
	{
		// create/customize the store according to your needs:
		
		var store = new DocumentStore
		{
			Urls = new[] { "http://127.0.0.1:8080" },
			Database = "linqpad-demo",
		}.Initialize();

		// log stuff to the sql pane
		store.GetRequestExecutor().OnBeforeRequest += async (s, e) => Log(e.Url);
		store.OnBeforeQuery += (s, e) => Log(e.QueryCustomization.ToString());

		static void Log(object message) => Util.SqlOutputWriter.WriteLine($"\r\n-- {DateTime.Now}\r\n-- {message}");
		return store;
	}

	using (DocumentStore = CreateDocumentStore())
	using (Session = DocumentStore.OpenSession())
		Main();
}

void Main() { }
