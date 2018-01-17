---
title: RecordGen0000
description: Rule title
category: Diagnostic category
severity: Hidden, Info, Warning, or Error
---

# This is a documentation stub

Please submit a PR with updates to the [appropriate file]({{ site.github.repository_url }}/tree/master/docs/{{ page.relative_path }}) or create an [issue](https://github.com/amis92/RecordGenerator/issues) if you see this.

## Cause

A concise-as-possible description of when this rule is violated. If there's a lot to explain, begin with "A violation of this rule occurs when..."

## Reason for rule

Explain why the user should care about the violation.

## How to fix violations

To fix a violation of this rule, [describe how to fix a violation].

## Examples

### Violates

Example(s) of code that violates the rule.

### Does not violate

Example(s) of code that does not violate the rule.

## How to suppress violations

**If the severity of your analyzer isn't _Warning_, delete this section.**

```csharp
#pragma warning disable RecordGen0000 // <Rule name>
#pragma warning restore RecordGen0000 // <Rule name>
```