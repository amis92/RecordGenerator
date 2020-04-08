# Amadevus.RecordGenerator

![RecordGenerator logo](https://raw.githubusercontent.com/amis92/RecordGenerator/master/docs/logo.png)

> ℹ This documentation is for v0.5 of RecordGenerator.

## Description
[Description]: #description

C# Record Generator makes creating **immutable** record types a breeze! Just adorn your data type with `[Record]` attribute and keep your code clean and simple. The backing code is generated on build-time, including IntelliSense support (just save the file, Visual Studio will make a build in background).

[![Join the chat at gitter!](https://img.shields.io/gitter/room/amis92/recordgenerator.svg)](https://gitter.im/amis92/RecordGenerator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/github/license/amis92/recordgenerator.svg)](https://github.com/amis92/RecordGenerator/blob/master/LICENSE)

[![NuGet package](https://img.shields.io/nuget/v/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages/Amadevus.RecordGenerator/)
[![NuGet package preview](https://img.shields.io/nuget/vpre/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages/Amadevus.RecordGenerator/)
[![MyGet package](https://img.shields.io/myget/amadevus/v/Amadevus.RecordGenerator.svg?label=myget-ci)](https://www.myget.org/feed/amadevus/package/nuget/Amadevus.RecordGenerator)

[![GitHub Actions - .NET CI workflow](https://github.com/amis92/RecordGenerator/workflows/.NET%20Core%20CI/badge.svg?branch=master)](https://github.com/amis92/RecordGenerator/actions?query=workflow%3A%22.NET+Core+CI%22+branch%3Amaster)
[![Azure Pipelines Build Status](https://dev.azure.com/amadevus/RecordGenerator/_apis/build/status/amis92.RecordGenerator?branchName=master)](https://dev.azure.com/amadevus/RecordGenerator/_build/latest?definitionId=1&branchName=master)

---

## Table of contents
[Table of contents]: #table-of-contents

* [Description]
* [Installation]
* [Usage]
* [Features]
* [Examples]
* [Diagnostics]
* [Requirements]
* [Development]
* [Contributions]

## Installation
[Installation]: #installation

As it is a NuGet, it's really simple:

* From terminal `dotnet add package Amadevus.RecordGenerator`
* Package Manager `Install-Package Amadevus.RecordGenerator`
* Or from `Manage NuGet packages` search for `Amadevus.RecordGenerator`

## Usage
[Usage]: #usage

```cs
using Amadevus.RecordGenerator;

namespace Example
{
    [Record]
    sealed partial class Foo
    {
        public string Bar { get; }
    }
}
```

As you can see, it's very nice and easy. You just have to **decorate your type
with `[Record]` attribute** and voilà, you have made yourself a record type!

Using generated features:

```csharp
using System;
using Amadevus.RecordGenerator;

namespace QuickDemo
{
    [Record(Features.Default | Features.Equality)]
    public sealed partial class Contact
    {
        public int Id { get; }
        public string Name { get; }
        public string Email { get; }
        public DateTime? Birthday { get; }
    }

    public static class Program
    {
        public static void Main()
        {
            var adam = new Contact.Builder
            {
                Id = 1,
                Name = "Adam Demo",
                Email = "foo@bar.com"
            }.ToImmutable();
            var adamWithBday = adam.WithBirthday(DateTime.UtcNow);
            Console.WriteLine("Pretty display: " + adamWithBday);
            // Pretty display: { Id = 1, Name = Adam Demo, Email = foo@bar.com, Birthday = 06.01.2020 23:17:06 }
            Console.WriteLine("Check equality: " + adam.Equals(adamWithBday));
            // Check equality: False
            Console.WriteLine("Check equality: " + adam.Equals(new Contact(1, "Adam Demo", "foo@bar.com", null)));
            // Check equality: True
        }
    }
}
```

The above is taken from [QuickDemo sample](../samples/QuickDemo/Program.cs)

##### What does it mean, a record type?

A record type is an immutable named container for a bunch of objects (properties). As it is,
C# doesn't provide an easy way to create immutable types - that's where RecordGenerator comes to
the rescue!

##### What do I get?

The generator creates new partial for your type with additional members. The generator first
acquires a list of **record entries** - public properties that are read-only and auto-implemented
(which basically means they're `public SomeType SomeName { get; }`). Then it generates additional
members, depending on features selected in `[Record(Features)]` attribute. (If nothing is selected, `Default` value is used)


## Features
[Features]: #features

The `[Flags] enum Features` has the following values:

Feature | Generated Members | Description
-|-|-
`Constructor` | `.ctor`, `OnConstructed` | Has a parameter for every record entry, and assigns those parameters to corresponding auto-properties. At the end, the partial method `OnConstructed` is invoked.
`Withers` | `WithBar`, `Update` | `With`-methods which take single record entry parameter and return new record instance with all values of record entries taken from current instance, except the parameter one which is changed to the parameter value. `Update` is a constructor forward.
`ToString` | `ToString` | Generates an override that replicates an anonymous class's `ToString` behavior.
`Builder` | `Builder`, `ToBuilder` | Nested class which has the same record entries as the record, but read-write, and a `ToImmutable` method that creates a record instance with builder's values. `ToBuilder` method returns a new builder instance with record's values copied.
`Deconstruct` | `Deconstruct` | Method which enables deconstruction of record into a list of variables like tuples do since C# 7.0 (ValueTuple). See Microsoft docs: [Deconstruct].
`ObjectEquals` | `object.Equals(object)`, `object.GetHashCode()` | Overrides that use record entries for comparisons and hash calculations.
`EquatableEquals` | `IEquatable<Foo>.Equals(Foo)` | Implements the interface.
`OperatorEquals` | `==`, `!=` | Implements the operators.
`Equality` | - | Bundle of `ObjectEquals`, `EquatableEquals`, `OperatorEquals` features.
`Default` | - | Bundle of all above features, except `Equality`.

[Deconstruct]: https://docs.microsoft.com/dotnet/csharp/deconstruct#deconstructing-user-defined-types


## Examples
[Examples]: #examples

* [Person](examples/Person.md)
* [Enclosed type (History.Entry)](examples/History.md)

## Diagnostics
[Diagnostics]: #diagnostics

The `Amadevus.RecordGenerator.Analyzers` package (pulled in by main package) provides
diagnostics/codefixes that help you use Records correctly. See [Analyzers].

## Requirements
[Requirements]: #requirements

![Visual Studio logo](https://upload.wikimedia.org/wikipedia/commons/6/61/Visual_Studio_2017_logo_and_wordmark.svg)

It is a development-only package, and the generation also works with CLI builds, both using `dotnet` and `msbuild`. The `Attributes` target `netstandard1.0` (the only compile-time dependecy).

It on depends `dotnet` CLI to run the underlying `CodeGeneration.Roslyn.Tool`,
which in turn requires `.NET Core App v2.1` runtime (or later).

Roslyn Analyzer with CodeFix, to be supported in IDE, requires **Visual Studio 2017+** or **VS Code v1.19+**.

If you want to use packages separately, there is more work to do.

* First of all, you can define your own `RecordAttribute` (instead of referencing the Attributes package);
  it needs to have the same name and `[CodeGeneration]` attribute applied,
  same as [the one defined in the Attributes package][RecordAttribute].
* The project where code will be generated needs to reference `Amadevus.RecordGeneration.Generators`
  and `CodeGeneration.Roslyn.Tool` packages.
* Analyzers package is optional.

## Development
[Development]: #development

To build the solution, .NET Core SDK v3.1.100 is required, as specified in `global.json`.

## Credits
[Credits]: #credits

`Amadevus.RecordGenerator` wouldn't work if not for @AArnott [AArnott's CodeGeneration.Roslyn](https://github.com/AArnott/CodeGeneration.Roslyn).

Analyzers in `Amadevus.RecordGenerator.Analyzers` were inspired by [xUnit.net's analyzers](https://github.com/xunit/xunit.analyzers).

## Contributions
[Contributions]: #contributions

All contributions are welcome, as well as critique. If you have any issues, problems or suggestions -
please open an issue.

Visual Studio logo ™ Microsoft Corporation, used without permission.

RecordGenerator logo (on top) © 2017 Amadeusz Sadowski, all rights reserved.

[Analyzers]: analyzers/
[RecordAttribute]: https://github.com/amis92/RecordGenerator/blob/339929215b7db49e3cb8824abfcb7c51243239b4/src/Amadevus.RecordGenerator.Attributes/RecordAttribute.cs
