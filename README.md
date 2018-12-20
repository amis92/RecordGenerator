# Amadevus.RecordGenerator

![RecordGenerator logo](https://raw.githubusercontent.com/amis92/RecordGenerator/master/docs/logo.png)

Documentation available at [amis92.github.io/RecordGenerator](https://amis92.github.io/RecordGenerator) (or in [docs folder](/docs/README.md)).

## Description
[Description]: #description

C# Record Generator makes creating record classes a breeze! Just adorn your data class with `[Record]` attribute
and keep your code clean and simple. The backing code is generated on build-time, including IntelliSense support
(just save the file, Visual Studio will make a build in background).

[![NuGet package](https://img.shields.io/nuget/v/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages/Amadevus.RecordGenerator/)
[![Build status](https://img.shields.io/appveyor/ci/amis92/recordgenerator/master.svg?label=build%20(master))](https://ci.appveyor.com/project/amis92/recordgenerator/branch/master)
[![MyGet package](https://img.shields.io/myget/amadevus/v/Amadevus.RecordGenerator.svg?label=myget-ci)](https://www.myget.org/feed/amadevus/package/nuget/Amadevus.RecordGenerator)
[![Join the chat at gitter!](https://img.shields.io/gitter/room/amis92/recordgenerator.svg)](https://gitter.im/amis92/RecordGenerator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/github/license/amis92/recordgenerator.svg)](https://github.com/amis92/RecordGenerator/blob/master/LICENSE)

---

## Development
[Development]: #development

To build the solution, .NET Core SDK v2.1.500 is required, as specified in `global.json`.

It must be built on Windows with MSBuild toolset (min. v15.0), since it targets Windows frameworks in Attributes package.

## Credits
[Credits]: #credits

`Amadevus.RecordGenerator` wouldn't work if not for @AArnott [AArnott's CodeGeneration.Roslyn](https://github.com/AArnott/CodeGeneration.Roslyn).

Analyzers in `Amadevus.RecordGenerator.Analyzers` were inspired by [xUnit.net's analyzers](https://github.com/xunit/xunit.analyzers).

## Contributions
[Contributions]: #contributions

All contributions are welcome, as well as critique. If you have any issues, problems or suggestions -
please open an issue.

When commiting a change, two main versioning mechanisms are branch name and version in `appveyor.yml`.
Branch name will be used as a suffix when publishing packages on MyGet feed. Version will be used for
both MyGet and releasing to NuGet. You might also update version in `Directory.Build.props` - that's used for non-CI builds.

Visual Studio logo ™ Microsoft Corporation, used without permission.

RecordGenerator logo (on top) © 2017 Amadeusz Sadowski, all rights reserved.