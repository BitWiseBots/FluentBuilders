# ![Logo](logo.png =32x) FluentBuilders
A .NET Standard library that provides a generic builder pattern capable of creating simple to complex object graphs. Designed to be used for unit testing.


### Build Status
[![CircleCI](https://circleci.com/gh/BitWiseBots/FluentBuilders/tree/main.svg?style=shield)](https://circleci.com/gh/BitWiseBots/FluentBuilders/tree/main)

## Getting Started
### Installation

To install using the NuGet package manager console within Visual Studio, run this command:
```
Install-Package BitWiseBots.FluentBuilders
```

Or to install using the .NET Core CLI from the command prompt:
```
dotnet add package BitWiseBots.FluentBuilders
```

### Creating a Builder

Lets say you have a class that looks something like:
```
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

You would create a builder, set its properties, and construct the final object like this:
```
Builders.Create<Person>
    .With(p => p.FirstName, "John")
    .With(p => p.LastName, "Doe")
    .Build()
```

This only scratches the surface of what FluentBuilders can accomplish.
If you have more complex objects or even graphs of objects check out our in depth usage guides [here](https://bitwisebots.github.io/FluentBuilders/).
