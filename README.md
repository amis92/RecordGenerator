# Amadevus.RecordGenerator

![RecordGenerator logo](https://raw.githubusercontent.com/amis92/RecordGenerator/master/docs/logo.png)

Documentation available at [amis92.github.io/RecordGenerator](https://amis92.github.io/RecordGenerator) (or in [docs folder](docs/README.md)).

## Description
[Description]: #description

C# Record Generator makes creating **immutable** record classes a breeze! Just adorn your data class with `[Record]` attribute
and keep your code clean and simple. The backing code is generated on build-time, including IntelliSense support
(just save the file, Visual Studio will make a build in background).

[![Join the chat at gitter!](https://img.shields.io/gitter/room/amis92/recordgenerator.svg)](https://gitter.im/amis92/RecordGenerator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/github/license/amis92/recordgenerator.svg)](https://github.com/amis92/RecordGenerator/blob/master/LICENSE)

[![NuGet package](https://img.shields.io/nuget/v/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages/Amadevus.RecordGenerator/)
[![NuGet package preview](https://img.shields.io/nuget/vpre/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages/Amadevus.RecordGenerator/)
[![MyGet package](https://img.shields.io/myget/amadevus/v/Amadevus.RecordGenerator.svg?label=myget-ci)](https://www.myget.org/feed/amadevus/package/nuget/Amadevus.RecordGenerator)

[![GitHub Actions - .NET CI workflow](https://github.com/amis92/RecordGenerator/workflows/.NET%20Core%20CI/badge.svg?branch=master)](https://github.com/amis92/RecordGenerator/actions?query=workflow%3A%22.NET+Core+CI%22+branch%3Amaster)
[![Azure Pipelines Build Status](https://dev.azure.com/amadevus/RecordGenerator/_apis/build/status/amis92.RecordGenerator?branchName=master)](https://dev.azure.com/amadevus/RecordGenerator/_build/latest?definitionId=1&branchName=master)

---

## Development
[Development]: #development

To build the solution, .NET Core SDK v2.1.500 is required, as specified in [`global.json`](global.json).

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