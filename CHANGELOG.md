# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

* Parameters of generated record methods use camel-casing, as opposed to
  Pascal-casing previously, to align with naming conventions in .NET.
* The name of the parameter to `With*` methods now always reads `value`
  as opposed to being named after the property verbatim.
* `RecordAttribute` is now in `Amadevus.RecordGenerator` namespace (previously in global/no namespace)

### Added

* [`Object.ToString`][objtostr] override for records that follows the same
  implementation as C# generates for anonymous types
* `GeneratedCodeAttribute` is added to generated non-type members (methods, properties)


[objtostr]: https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring
