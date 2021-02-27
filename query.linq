<Query Kind="Statements">
  <Namespace>Example.Entities</Namespace>
</Query>

// load the db connection
#load ".\connection"

// load entity types
#load ".\*.cs"

// The following examples were adapted from the RavenDB docs

// These first 2 Examples will use the query's own Session instance...
var productId = Example_StoreSomeStuff_Returning_Id();
Example_LoadStuff_And_Update(productId);

// ... but you can create your own sessions if you want
using (var session = DocumentStore.OpenSession())
{
	// example query
	var productNames = session
	   .Query<Product>()
	   .Where(x => x.UnitsInStock > 5)
	   .Skip(0).Take(10)
	   .Select(x => x.Name)
	   .ToList()
	   .Dump();
}

string Example_StoreSomeStuff_Returning_Id()
{
	var category = new Category
	{
		Name = $"Category {Guid.NewGuid()}"
	};

	Session.Store(category);

	var product = new Product
	{
		Name = $"Product {Guid.NewGuid()}",
		Category = category.Id,
		UnitsInStock = 10
	};

	Session.Store(product);
	Session.SaveChanges();

	return product.Id;
}

void Example_LoadStuff_And_Update(string productId)
{
	var product = Session
		.Include<Product>(x => x.Category)
		.Load(productId);

	var category = Session.Load<Category>(product.Category);

	product.Name = $"Product {Guid.NewGuid()}";
	category.Name = $"Category {Guid.NewGuid()}";

	Session.SaveChanges();
}