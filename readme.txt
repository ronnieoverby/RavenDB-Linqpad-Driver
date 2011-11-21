RavenDB LINQPadDriver

Yeah... a RavenDB driver for LINQPad.

Uses RavenDB client build 531.
Referencing another version of the client in LINQPad SHOULD take precedence.

=========


To install:

1) Download RavenDB_LINQPad_Driver.zip

2) Extract lpx files.

3) In Linqpad: Add Connection, View more drivers, browse for lpx file.

4) Complete the connection information form.

5) Create a new query using the raven connection that you just defined.

6) Press F4 to add references to the assemblies containing your entity types OR create entity classes in Linqpad.

7) You're done. You can write some code against your Raven database now.

---------

Use the driver that aligns with your entity assembly version.
IE. if your entities are in 3.5 assembly, use the 3.5 version.