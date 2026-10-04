---
layout: docs
title: Serialize a builder
parent: .NET SDK
nav_order: 60
description: Render an ExpressionBuilder pipeline or PredicationBuilder rule as Expressif source text.
---

`ToSource()` renders a programmatically composed pipeline or rule as portable Expressif source. It does not consume the builder state.

## Render an expression pipeline

```csharp
var environment = ExpressifEnvironment.Default;
var pipeline = environment.CreateExpressionBuilder()
    .Create<Lower>()
    .Then<FirstChars>(5)
    .Then<PadRight>(7, '*');

var source = pipeline.ToSource();
// lower | first-chars(5) | pad-right(7, *)
```

## Render a predication rule

```csharp
var builder = environment.CreatePredicationBuilder();
var rule = builder
    .Create<StartsWith>("ola")
    .Or(builder.Create<EndsWith>("sla").Not());

var source = rule.ToSource();
```

The renderer includes the grouping required to preserve the Boolean structure.

## Build and render in any order

Both staged models are reusable. `Build()` and `ToSource()` may be called repeatedly and in either order:

```csharp
var sourceBefore = pipeline.ToSource();
var first = pipeline.Build();
var second = pipeline.Build();
var sourceAfter = pipeline.ToSource();
```

The source represents the Expressif rule, not its `EvaluationContext`. Host values and providers must be stored separately when another process needs them.
