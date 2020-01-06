---
title: RecordGen1002
description: Record properties cannot differ only by case
category: Usage
severity: Error
---

## Cause

The type marked with `[Record]` has properties with names that differ only by case, like `Name` and `name`.

## Reason for rule

Generated constructor has parameters with names that are their corresponding properties'
but with the first letter lower-cased. There may also exist other places where casing is
changed to accomodate for .NET naming conventions, like `Deconstruct` parameter names.

## How to fix violations

To fix a violation of this rule, rename one of the properties so that they're different
not only by letter case.

## Examples

### Violates

```csharp
using Amadevus.RecordGenerator;

[Record]
sealed partial class DataClass
{
    public string name { get; }

    public string Name { get; }
}
```

### Does not violate

```csharp
using Amadevus.RecordGenerator;

[Record]
sealed partial class DataClass
{
    public string FirstName { get; }

    public string Name { get; }
}
```
