There's no linqpad driver for RavenDB 4+, yet.

Recent versions of Linqpad allow loading external .cs and .linq files.

This branch demonstrates how to define a "database connection" in one query, `connection.linq`.

In another query, `query.linq`, the connection and entity types are loaded and then used to interact with the database.

Enjoy!