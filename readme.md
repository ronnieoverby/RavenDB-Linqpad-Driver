#RavenDB LINQPad Driver

Releases are hosted on Github: https://github.com/ronnieoverby/RavenDB-Linqpad-Driver/releases

The latest release uses RavenDB client build 3183.

Watch the video at http://youtu.be/XgsPvyk0bjM for help getting started.

---

To install:

1) In Linqpad, click "Add connection".

2) In the "Choose Data Context" dialog, press the "View more drivers..." button.

3) In the "Choose a Driver" dialog, find the "RavenDB Driver" and click the "Download & Enabled driver" link.

4) Back in the "Choose Data Context" dialog, select "RavenDB Driver" and click the next button.

5.) In the RavenDB connection dialog, supply your connection information.

6) You're done. You can write some code against your Raven database now. For example:

```c#
// c# expression
from a in Query<Album>()
where a.Title.StartsWith("Classic")
select a
```


The RavenDB Linqpad Driver will create a DocumentStore for you to use to connect to RavenDB.
It uses the details that you supply in the connection properties window inside Linqpad.
If you need to fully control the creation of the IDocumentStore (for sharding, etc) you can do it:

1) In your project, add a reference to RavenLinqpadDriver.Common.dll (can be found in /Common)

2) Implement ICreateDocumentStore

3) In Linqpad's RavenDB connection properties, reference your project's assembly.

4) When you run queries under that Linqpad connection, you're custom IDocumentStore will be used.

### Thanks

Special thanks to [JetBrains](http://www.jetbrains.com/) for supporting this project by donating a license of [ReSharper](http://www.jetbrains.com/resharper/)!
