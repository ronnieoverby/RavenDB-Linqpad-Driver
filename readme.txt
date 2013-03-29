RavenDB LINQPadDriver

The latest release uses RavenDB client build 2330 and can be downloaded at http://goo.gl/T5hfm

Referencing another version of the client in LINQPad SHOULD take precedence.

Watch the video at http://youtu.be/XgsPvyk0bjM for help getting started.

=========

To install:

1) Download RavenLinqpadDriver x.x.x.x.zip

2) Extract lpx files.

3) In Linqpad: Add Connection, View more drivers, browse for lpx file.

4) Complete the connection information dialog. Fill out RavenDB URL, default database, your assembly paths/namespaces, etc.

5) Create a new query using the raven connection that you just defined.

6) You're done. You can write some code against your Raven database now. For example:

// c# expression
from a in Query<Album>()
where a.Title.StartsWith("Classic")
select a

=========

The RavenDB Linqpad Driver will create a DocumentStore for you to use to connect to RavenDB.
It uses the details that you supply in the connection properties window inside Linqpad.
If you need to fully control the creation of the IDocumentStore (for sharding, etc) you can do it:

1) In your project, add a reference to RavenLinqpadDriver.Common.dll (can be found in /Common)

2) Implement ICreateDocumentStore

3) In Linqpad's RavenDB connection properties, reference your project's assembly.

4) When you run queries under that Linqpad connection, you're custom IDocumentStore will be used.