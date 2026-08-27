---
layout: docs
title: Evaluate a predication
parent: .NET SDK
nav_order: 30
description: Evaluate an Expressif Boolean rule from C# with a strongly typed result.
---

A `Predication` is the strongly typed .NET API for an Expressif rule that returns a Boolean value.

Create it once, then evaluate each input with the same rule:

```csharp
var predication = new Predication("lower-case");

bool first = predication.Evaluate("Nikola Tesla");
bool second = predication.Evaluate("nikola tesla");
```

`first` is `false`; `second` is `true`.

The added value over `Expression.Create(...)` is the return type:

```csharp
object? expressionResult = Expression.Create("lower-case")
    .Evaluate("nikola tesla");

bool predicationResult = new Predication("lower-case")
    .Evaluate("nikola tesla");
```

Use `Predication` when the consuming C# code benefits from a guaranteed `bool`. Use `Expression.Create(...)` when the result may have another type or when one evaluation abstraction is more convenient across the application.

See [Predicates](../language/predicates.md) for predicate parameters, negation, Boolean operators, and grouping syntax.
