---
title: RecordGen1000
description: Record type and all containing types must be partial
category: Usage
severity: Error
---

## Cause

The type marked with `[Record]` (or its containing type) is not `partial`.

## Reason for rule

Code generation creates a partial type with backing code in another file. If this rule is violated,
there will be a compilation error [CS0260](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs0260).

## How to fix violations

To fix a violation of this rule, add `partial` modifier to offending types (there is a provided codefix).

## Examples

### Violates

```csharp
using Amadevus.RecordGenerator;

[Record]
struct DataClass
{
    public string Name { get; }
}

// both violate, outer and inner
class OuterClass
{
    [Record]
    struct InnerClass
    {
        public string Name { get; }
    }
}
```

### Does not violate

```csharp
using Amadevus.RecordGenerator;

[Record]
partial struct DataClass
{
    public string Name { get; }
}

// both are partial
partial class OuterClass
{
    [Record]
    partial struct InnerClass
    {
        public string Name { get; }
    }
}
```
