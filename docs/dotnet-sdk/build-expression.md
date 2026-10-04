---
layout: docs
title: Build an expression with C#
parent: .NET SDK
nav_order: 40
description: Compose a reusable Expressif function pipeline programmatically with ExpressionBuilder.
---

Use `ExpressionBuilder` when the transformation is part of the program itself. Use `CreateExpression(...)` when the transformation arrives as text.

```csharp
var environment = ExpressifEnvironment.Default;

var pipeline = environment.CreateExpressionBuilder()
    .Create<Lower>()
    .Then<FirstChars>(5);

var expression = pipeline.Build();
var result = expression.Evaluate("Nikola Tesla"); // "nikol"
```

`ExpressionBuilder` is only the starter. `Create(...)` produces a non-empty `ExpressionBuilder.Pipeline`; only that pipeline exposes `Then(...)`, `Build()`, and `ToSource()`. An empty buildable pipeline cannot be represented through the normal public API.

## Pass literal parameters

Arguments passed to `Create<T>(...)` or `Then<T>(...)` retain their runtime types:

```csharp
var pipeline = environment.CreateExpressionBuilder()
    .Create<PadRight>(15, '*');

var expression = pipeline.Build();
var result = expression.Evaluate("Nikola Tesla"); // "Nikola Tesla***"
```

The function type must implement `IFunction`. Runtime-type overloads are available when the type is selected dynamically:

```csharp
var pipeline = environment.CreateExpressionBuilder()
    .Create(typeof(Lower))
    .Then(typeof(FirstChars), 5)
    .Then(typeof(PadRight), 7, '*');
```

## Branch and compose pipelines

Pipelines are immutable. `Then(...)` returns a new value and leaves the earlier pipeline unchanged:

```csharp
var builder = environment.CreateExpressionBuilder();
var lower = builder.Create<Lower>();
var shortName = lower.Then<FirstChars>(5);

var lowerResult = lower.Build().Evaluate("Nikola Tesla");       // "nikola tesla"
var shortResult = shortName.Build().Evaluate("Nikola Tesla"); // "nikol"
```

Compose pipelines created by the same builder:

```csharp
var middle = builder
    .Create<FirstChars>(5)
    .Then<PadRight>(7, '*');

var complete = builder
    .Create<Lower>()
    .Then(middle)
    .Then<Upper>();
```

`Then(Pipeline)` appends the child stages without building or consuming it. Pipelines from different builder/factory instances are rejected because they may use different registered libraries.

## Supply an argument provider

Use `Argument.From<T>(...)` when an argument depends on the current immutable evaluation scope:

```csharp
var pipeline = environment.CreateExpressionBuilder()
    .Create<Append>(Argument.From<string>(scope =>
        scope.GetVariable<string>("suffix")));

var context = EvaluationContext.CreateBuilder()
    .AddValue("suffix", "!")
    .Build();

var result = pipeline.Build().WithContext(context).Evaluate("Hello"); // "Hello!"
```

The scope exposes `Current`, `Root`, `EnclosingRoot`, and read-only variable lookup. The operator's argument-evaluation contract determines how often the provider runs. An `Argument.From<T>(...)` provider is executable host code and cannot be represented as Expressif source, so `ToSource()` throws `NotSupportedException` for a pipeline containing one.

## Reuse and render a pipeline

`Build()` is non-consuming. Repeated calls return independent executable expressions. For pipelines whose arguments can be represented as Expressif source, `ToSource()` works before or after any build:

```csharp
var pipeline = environment.CreateExpressionBuilder()
    .Create<Lower>()
    .Then<Length>();

var source = pipeline.ToSource();
var first = pipeline.Build();
var second = pipeline.Build();
```

See [Serialize a builder](../serialization/) for source rendering.
