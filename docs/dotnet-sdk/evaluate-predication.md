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
var environment = ExpressifEnvironment.Default;
var predication = environment.CreatePredication("lower-case");

bool first = predication.Evaluate("Nikola Tesla");
bool second = predication.Evaluate("nikola tesla");
```

`first` is `false`; `second` is `true`.

The added value over `CreateExpression(...)` is the return type:

```csharp
object? expressionResult = environment.CreateExpression("lower-case")
    .Evaluate("nikola tesla");

bool predicationResult = environment.CreatePredication("lower-case")
    .Evaluate("nikola tesla");
```

Use `Predication` when the consuming C# code benefits from a guaranteed `bool`. Use `CreateExpression(...)` when the result may have another type or when one evaluation abstraction is more convenient across the application. Both methods resolve operators from the same immutable environment.

See [Predicates](../language/predicates.md) for predicate parameters, negation, Boolean operators, and grouping syntax.
