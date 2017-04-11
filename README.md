![RecordGenerator logo](/docs/logo.png)

## Amadevus.RecordGenerator

#### Description
[Description]: #description

C# RecordGenerator Analyzer with record generating CodeFix. And the analyzer keeps watch over
your code to make sure the generated partial is always up-to-date, offering you another codefix
in case regeneration is due! How awesome is that?

[![NuGet package](https://img.shields.io/nuget/v/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages/Amadevus.RecordGenerator/)
[![Build status](https://img.shields.io/appveyor/ci/amis92/recordgenerator.svg)](https://ci.appveyor.com/project/amis92/recordgenerator/branch/master)
[![MyGet package](https://img.shields.io/myget/amadevus/v/Amadevus.RecordGenerator.svg?label=myget-ci)](https://www.myget.org/feed/amadevus/package/nuget/Amadevus.RecordGenerator)
[![Join the chat at gitter!](https://img.shields.io/gitter/room/amis92/recordgenerator.svg)](https://gitter.im/amis92/RecordGenerator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/github/license/amis92/recordgenerator.svg)](https://github.com/amis92/RecordGenerator/blob/master/LICENSE)
---

#### Table of content
[Table of content]: #table-of-content

* [Description]
* [Installation]
* [Usage]
* [Requirements]
* [Contributions]

#### Installation
[Installation]: #installation

As it is a NuGet, it's really simple:

* Package Manager `Install-Package Amadevus.RecordGenerator`
* Terminal (.NET Core) `dotnet add package Amadevus.RecordGenerator`

#### Usage
[Usage]: #usage

Watch the gif below and then read on:

! [Demo gif] (/docs/demo.gif)

As you can see, it's very nice and easy. You just have to **decorate your class or struct
with `[Record]` attribute** and voila, you have a diagnostic with codefix available
at class identifier. If you haven't already declared such an attribute somewhere,
there is also a diagnostic with codefix that will do that for you - also show in demo above.

As presented, the generator creates new partial type with some members. Generator processes all
properties that are read-only and auto-implemented (which basically means they're
`public SomeType SomeName { get; }`) - from now on we'll call them record entries. Then there are
currently two kinds of members generated:

1. Constructor that has a parameter for every record entry, and assigns those parameters
   to corresponding auto-properties.
2. `With` mutators which take single record entry parameter and return new record instance
   with all values of record entries taken from current instance, except the parameter one which 
   is changed.

Additionally, after such file is generated, the analyzer monitors changes and gives error
if regeneration is required. That error also has available codefix which will update generated code.

#### Requirements
[Requirements]: #requirements

As this is a Roslyn Analyzer with CodeFix, it requires **Visual Studio 2015+** (it should also work
with **VS Code**).

#### Contributions
[Contributions]: #contributions

All contributions are welcome, as well as critique. If you have any issues, problems or suggestions -
please open an issue.
