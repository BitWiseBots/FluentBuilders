# BitWiseBots.FluentBuilders
## Builders

### Getting Started

1. Find a class you want to build. Note that this class is immutable but that is no problem.
```csharp
public class User
{
    public User(string firstName, string lastName, int age)
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public int Age { get; }
}
```
2. Register your builders. This one builds an immutable class via the constructor. Builder registration classes should be placed within your `Test` or `TestSupport` project. The `BuilderRegistrationsManager` must be called to initialize the registrations before any tests are run. See more detail about this process below.
```csharp
public class SampleBuilderRegistration : BuilderRegistration
{
    public SampleBuilderRegistration()
    {
      RegisterBuilder<User>(b => new User(
        b.From(u => u.FirstName),
        b.From(u => u.lastName),
        b.From(u => u.Age)));
    }
}
```
3. Initialize the registrations before tests are run.
```csharp
[SetUpFixture]
public class AllTestsFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BuilderRegistrationsManager.AddBuilderRegistration<SampleBuilderRegistration>();
    }
}
```
4. Use the builder in a test.
```csharp
[Test]
public void ExampleConstructedTest()
{
    var user = BuilderFactory.Create<User>()
           .With(u => u.FirstName, "John")
           .With(u => u.LastName, "Smith")
           .With(u => u.Age, 23)
           .Build();
}
```

### What Is It?
This is a simple library for creating builders for types used within tests. Often a test "arrange" section requires the instantiation of a variety of configured objects to exercise the class under test.

### Why Use It?
One approach is to manually create a builder class for each object you need, but this library provides support for dynamically creating builders. This means you don't need to create one builder class per type.

### Usage


#### Registration
For simple types, you don't need to perform any up-front initialization. For example, if you have a simple model like this:
```csharp
public class UserModel
{
  public string FirstName { get; set; }
}
```
then you can use a builder dynamically without any registration:
```csharp
var model = BuilderFactory.Create<UserModel>()
  .With(u => u.FirstName, "John")
  .Build();
```
However, immutable types require registration. Registration also allows you to configure default values for properties.

For example, assume you have the following classes:
```csharp
public class Request
{
  public string Name { get; set; }
  public string Description { get; set; }
}

public class Message : IMessage
{
    public Message(Guid integrationId, Request request)
    {
        IntegrationId = integrationId;
        Id = Guid.NewGuid();
        Request = request;
    }
    public Guid Id { get; }
    public Guid IntegrationId { get; }
    public Request Request { get; }
}
```
In order to use a builder for `Message` it needs to be registered in advance. Registrations need to be implemented in a class that inherits from `BuilderRegistration`. You can have as many of these as you like. It's a good idea to organize builder registrations around a logical area of functionality in the system.

To register a builder for `Message` you would implement a constructor like this inside a new class that inherits from `BuilderRegistration`:
```csharp
public SampleBuilderRegistration()
{
  RegisterBuilder<Message>(b =>
    new Message(
        b.From(m => m.IntegrationId, Guid.NewGuid()),
        b.From(m => m.Request => command.Request, BuilderFactory.Create<Request>())));
}
```
This registration defines a builder for `Message` that uses the constructor and initializes the `IntegrationId` to a new `Guid` and also initializes the `Request` by using another dynamic builder.

Once you have one or more implementations of `BuilderRegistration` the `BuilderRegistrationsManager` needs to be told about them, there are multiple ways to achieve this that are demonstrated below.

###### Individually using generics
```csharp
[SetUpFixture]
public class AllTestsFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BuilderRegistrationsManager.AddBuilderRegistration<SampleBuilderRegistration>();
    }
}
```

###### Individually using a `System.Type` instance
```csharp
[SetUpFixture]
public class AllTestsFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BuilderRegistrationsManager.AddBuilderRegistration(typeof(SampleBuilderRegistration));
    }
}
```

###### Individually using a `BuilderRegistration` instance
```csharp
[SetUpFixture]
public class AllTestsFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BuilderRegistrationsManager.AddBuilderRegistration(new SampleBuilderRegistration());
    }
}
```

###### Collectively by scanning multiple assemblies
```csharp
[SetUpFixture]
public class AllTestsFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BuilderRegistrationsManager
            .AddBuilderRegistrations(Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly());
    }
}
```

###### Collectively by scanning multiple assemblies by name
```csharp
[SetUpFixture]
public class AllTestsFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BuilderRegistrationsManager
            .AddBuilderRegistrations("BitWiseBots.FluentBuilders.Tests.Support", "BitWiseBots.FluentBuilders.Tests.Unit");
    }
}
```

###### Collectively by scanning multiple assemblies by type
```csharp
[SetUpFixture]
public class AllTestsFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BuilderRegistrationsManager
            .AddBuilderRegistrations(typeof(SampleBuilderRegistration), typeof(OtherSampleBuilderRegistration));
    }
}
```

Note that in all these examples the registration was performed in the `OneTimeSetUp` method, this is done on purpose as attempting to perform the same registrations multiple times will cause an exception to be thrown.

#### Default Values
Registration can also be used as a way to provide default values using the object initializer.
```csharp
public SampleBuilderRegistration()
{
  RegisterBuilder<UserModel>(b =>
    new UserModel()
        {
            Id = b.From(u => u.Id, Guid.NewGuid())
        });
}
```

#### Post Build Actions
As of version `1.1.0` support has been added for registering an action to be performed after build has completed.
These registrations will be added within a Builder Registration implementation just like a constructor registration
```csharp
public SampleBuilderRegistration()
{
  RegisterPostBuildAction<UserModel>(u => u.ClientId = u.Client.Id);
}
```

#### Creation
All builders should be created by calling the `BuilderFactory`. Call `Build()` on the builder to get an instance of the type.
```csharp
var message = BuilderFactory.Create<CategoryIncludedMessage>()
                .Build();
```
#### Setting properties
The object that is built by the factory can be configured using the `With()` method. The first parameter is a lambda expression for the property to be set and the second parameter is the value the property should take.
```csharp
    var user = BuilderFactory.Create<User>()
           .With(u => u.FirstName, "John")
           .Build();
```

Even if the object is immutable like this, properties can be set as long as a getter is available.
```csharp
class User
{
  public User(string firstName)
  {
    FirstName = firstName;
  }
  public string FirstName { get; }
}
```

#### Params properties
When creating an instance of a type that has enumerable properties you simply provide one or more values. In this example the `CreateLmsUserCommand` has an `AdminAccessRules` property which can take multiple values.
```csharp
var command = BuilderFactory.Create<CreateLmsUserCommand>()
        .With(c => c.AdminAccessRules, new AdminAccessRule(), new AdminAccessRule())
        .Build();
```

#### Nested builders
When creating an instance of a more complex type, you can nest builders. In this example the `RefineCategoryCommand` has a `Category` property which can be populated by its own builder.
```csharp
var command = BuilderFactory.Create<RefineCategoryCommand>()
        .With(c => c.Category, BuilderFactory.Create<LmsCategoryResource>().Build())
        .Build();
```
You can also, in simple cases as seen above, omit the call to `Build()` and the `BuilderFactory` will take care of calling it for you.
```csharp
var command = BuilderFactory.Create<RefineCategoryCommand>()
        .With(c => c.Category, BuilderFactory.Create<LmsCategoryResource>())
        .Build();
```
Finally you can also use this for the params properties shown above.
```csharp
var command = BuilderFactory.Create<CreateLmsUserCommand>()
        .With(c => c.AdminAccessRules,
            BuilderFactory.Create<AdminAccessRule>(),
            BuilderFactory.Create<AdminAccessRule>()))
        .Build();
```

#### Advanced Usage Details

###### Deep Object Graphs
If your object to be built can have multiple layers of children then it is important to know that under most conditions the `BuilderFactory` can automatically instantiate intermediate layers.

For example, let's say we have an object that looks like this:
```csharp
public class Employee
{
	public string FirstName {get; set;}
	public string LastName {get; set;}

	public Employee Supervisor {get; set;}
}
```
This object lets us have a supervisor chain of any length, so lets say we want test day with 3 layers of supervisors but for whatever reason we don't really care about the data in the middle levels. 
A naive approach might look something like this
```csharp
var employee = BuilderFactory.Create<Employee>()
		.With(e => e.FirstName, "John")
		.With(e => e.LastName, "Doe")
		.With(e => e.Supervisor, 
			BuilderFactory.Create<Employee>()
				.With(e2 => e2.Supervisor, 
					BuilderFactory.Create<Employee>()
						.With(e3 => e3.FirstName, "Jane")
						.With(e3 => e3.LastName, "Smith")
						.Build()
				.Build())
		.Build());
```
Instead this could be simplified like so:
```csharp
var employee = BuilderFactory.Create<Employee>()
		.With(e => e.FirstName, "John")
		.With(e => e.LastName, "Doe")
		.With(e => Supervisor.Supervisor.Supervisor, 
				BuilderFactory.Create<Employee>()
					.With(e2 => e2.FirstName, "Jane")
					.With(e2 => e2.LastName, "Smith"))
		.Build()
```
This will cause the builder to create a new `Employee` object for each supervisor level specified, and if you'll noticed we even added an extra layer over the original example

###### Property Accessibility
While the `BuilderFactory` requires that a property has a public getter, the setter can be marked `private` and the factory will still be able to set that property.

###### Indexed Properties
Version `2.0.0` brought with it the ability to use indexed properties within any `With()` lambda, allowing easier setting of dictionaries, lists, or custom indexers.
```csharp
var obj = BuilderFactory.Create<Dictionary<string,string>>()
		.With(d => d["SomeKey"], "SomeValue")
		.Build();
```
This can also be applied to complex types being returned by the indexer, the following examples are roughly equivalent:
```csharp
var obj = BuilderFactory.Create<Dictionary<string, User>>()
		.With(d => d["SomeUserId"].FirstName, "Bob")
		.Build();

var obj = BuilderFactory.Create<Dictionary<string, User>>()
		.With(d => d["SomeUserId"], BuilderFactory.Create<User>.With(u => u.FirstName, "Bob)
		.Build();
```
Or even complex types used as keys
```csharp
var obj = BuilderFactory.Create<Dictionary<User, string>>()
		.With(d => d[new User{Id="someId"}], "John Doe")
		.Build();
```
Finally they key value provided to the indexer doesn't need to be static it can, for example, be provided by a delegate
```csharp
Func<string> keyDelegate = () => "someKey";
var obj = BuilderFactory.Create<Dictionary<string,User>>()
		.With(d => d[keyDelegate()], new User())
		.Build()
```
