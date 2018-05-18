# Amadevus.RecordGenerator

![RecordGenerator logo](https://raw.githubusercontent.com/amis92/RecordGenerator/master/docs/logo.png)

## Description
[Description]: #description

C# Record Generator makes creating record classes a breeze! Just adorn your data class with `[Record]` attribute and keep your code clean and simple. The backing code is generated on build-time, including IntelliSense support (just save the file, Visual Studio will make a build in background).

[![NuGet package](https://img.shields.io/nuget/v/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages/Amadevus.RecordGenerator/)
[![Build status](https://img.shields.io/appveyor/ci/amis92/recordgenerator.svg)](https://ci.appveyor.com/project/amis92/recordgenerator/branch/master)
[![MyGet package](https://img.shields.io/myget/amadevus/v/Amadevus.RecordGenerator.svg?label=myget-ci)](https://www.myget.org/feed/amadevus/package/nuget/Amadevus.RecordGenerator)
[![Join the chat at gitter!](https://img.shields.io/gitter/room/amis92/recordgenerator.svg)](https://gitter.im/amis92/RecordGenerator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/github/license/amis92/recordgenerator.svg)](https://github.com/amis92/RecordGenerator/blob/master/LICENSE)

---

## Table of contents
[Table of contents]: #table-of-contents

* [Description]
* [Installation]
* [Usage]
* [Examples]
* [Diagnostics]
* [Requirements]
* [Contributions]

## Installation
[Installation]: #installation

As it is a NuGet, it's really simple:

* Package Manager `Install-Package Amadevus.RecordGenerator`
* Or from `Manage NuGet packages` search for `Amadevus.RecordGenerator`

You will also need to add a `DotNetCliToolReference` to your `.csproj` file
(it's best to use the same version that `Amadevus.RecordGenerator` depends on of `CodeGeneration.Roslyn.BuildTime`):

```xml
    <Project>
      ...
      <ItemGroup>
        <DotNetCliToolReference Include="dotnet-codegen" Version="0.4.49" />
      </ItemGroup>
    </Project>
```

## Usage
[Usage]: #usage

```cs
namespace TestApp
{
    [Record]
    partial class RecordTest
    {
        public string Name { get; }
    }
}
```

As you can see, it's very nice and easy. You just have to **decorate your class
with `[Record]` attribute** and voilà, you have made yoursef a record type!

##### What does it mean, a record type?

A record type is an immutable named container for a bunch of objects (properties). As it is,
C# doesn't provide an easy way to create immutable types - that's where RecordGenerator comes to
the rescue!

##### What do I get?

The generator creates new partial for your type with additional members. The generator first
acquires a list of record entries - properties that are read-only and auto-implemented
(which basically means they're `public SomeType SomeName { get; }`). Then it generates additional
members which make using your record actually possible. These are currently:

1. Constructor that has a parameter for every record entry, and assigns those parameters
   to corresponding auto-properties.
2. `With` mutators which take single record entry parameter and return new record instance
   with all values of record entries taken from current instance, except the parameter one which 
   is changed.
3. `Update` method which essentially wraps constructor.
4. `Builder` nested class which has read-write properties same as record type and a `ToImmutable`
    method which returns a new record instance with builder's properties copied.
5. `ToBuilder` method which creates a builder with record's properties copied.
6. `Deconstruct` method which enables deconstruction of record into a list of variables like
    tuples do since C# 7.0 (ValueTuple).
7. `Validate` partial method called from constructor after all properties are assigned. You may
    (but don't have to) implement it - this would be the place to check for null arguments or similar.

## Examples
[Examples]: #examples

* [Person](examples/Person.md)
* [Enclosed type](examples/History.md)

## Diagnostics
[Diagnostics]: #diagnostics

The `Amadevus.RecordGenerator.Analyzers` package (pulled in by main package) provides
diagnostics/codefixes that help you use Records correctly. See [Analyzers].

* [RecordGen1000](analyzers/rules/RecordGen1000.md) RecordMustBePartial with a codefix.

## Requirements
[Requirements]: #requirements

![Visual Studio logo](https://upload.wikimedia.org/wikipedia/commons/6/61/Visual_Studio_2017_logo_and_wordmark.svg)

It is a `netstandard1.6` package, and the generation also works with CLI builds, both using `dotnet` and directly `msbuild`.

As this contains a Roslyn Analyzer with CodeFix, it requires **Visual Studio 2015+** (~~it should also work
with **VS Code**~~ nope, until [OmniSharp/omnisharp-vscode#43](https://github.com/OmniSharp/omnisharp-vscode/issues/43)).

If you want to use packages separately, there is more work to do.

* First of all, you can define your own `RecordAttribute`, it needs to have the same name
  and `[CodeGeneration]` attribute applied, same as [the one defined in the Attributes package](https://github.com/amis92/RecordGenerator/blob/master/src/Amadevus.RecordGenerator.Attributes/RecordAttribute.cs).
* The project where code will be generated needs to reference `Amadevus.RecordGeneration.Generators`
  and `CodeGeneration.Roslyn.BuildTime` packages, as well as `dotnet-codegen` DotNetCliTool of the same version
  that main package (Amadevus.RecordGenerator) of given version does.
* Analyzers package is optional.
* If you declare your own RecordAttribute (see above) Attributes package is optional too.

## Credits
[Credits]: #credits

`Amadevus.RecordGenerator` wouldn't work if not for @AArnott [AArnott's CodeGeneration.Roslyn](https://github.com/AArnott/CodeGeneration.Roslyn).

Analyzers in `Amadevus.RecordGenerator.Analyzers` were inspired by [xUnit.net's analyzers](https://github.com/xunit/xunit.analyzers).

## Contributions
[Contributions]: #contributions

All contributions are welcome, as well as critique. If you have any issues, problems or suggestions -
please open an issue.

When commiting a change, two main versioning mechanisms are branch name and version in `appveyor.yml`. Branch name will be used as a suffix when publishing packages on MyGet feed. Version will be used for both MyGet and releasing to NuGet. You might also update version in `Directory.Build.props` - that's used for non-CI builds.

Visual Studio logo ™ Microsoft Corporation, used without permission.

RecordGenerator logo (on top) © 2017 Amadeusz Sadowski, all rights reserved.

[Analyzers]: analyzers/