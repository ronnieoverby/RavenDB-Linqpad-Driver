RavenDB LINQPadDriver

Uses RavenDB client build 888.
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