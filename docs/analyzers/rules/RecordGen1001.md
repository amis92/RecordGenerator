---
title: RecordGen1001
description: Record must be sealed if equality generation is enabled
category: Usage
severity: Warning
---

## Cause

The class marked with `[Record]` is not `sealed` and equality generation is enabled.

## Reason for rule

Generated equality implementation for classes is not inheritance-friendly,
and there'd be no guarantee it's in any shape or form correct in derived classes.
Thus, it can only be generated for sealed classes.

## How to fix violations

To fix a violation of this rule, either:
* add `sealed` modifier to offending types (there is a provided codefix);
* disable equality generation for this type: `[Record(Features.Default & ~Features.Equality)]`.

## Examples

### Violates

```csharp
using Amadevus.RecordGenerator;

[Record(Features.Equality)]
partial class DataClass
{
    public string Name { get; }
}
```

### Does not violate

```csharp
using Amadevus.RecordGenerator;

[Record(Features.Equality)]
sealed partial class DataClass
{
    public string Name { get; }
}


[Record(Features.Default & ~Features.Equality)]
partial class BaseDataClass
{
    public string Name { get; }
}
```
