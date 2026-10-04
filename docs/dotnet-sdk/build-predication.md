---
layout: docs
title: Build a predication with C#
parent: .NET SDK
nav_order: 50
description: Compose reusable predicates, negation, and Boolean groups with PredicationBuilder.
---

`PredicationBuilder` is a starter for a non-empty Boolean rule:

```csharp
var environment = ExpressifEnvironment.Default;
var rule = environment.CreatePredicationBuilder()
    .Create<StartsWith>("Nik")
    .And<EndsWith>("sla");

var predicate = rule.Build();
var result = predicate.Evaluate("Nikola Tesla"); // true
```

Only `PredicationBuilder.Rule` exposes Boolean composition, `Build()`, and `ToSource()`, so an empty predication cannot be built through the normal public API.

## Combine and group rules

Use `And<T>(...)`, `Or<T>(...)`, and `Xor<T>(...)` for positive leaf predicates. Use their rule-taking overloads for explicit grouping:

```csharp
var builder = environment.CreatePredicationBuilder();

var suffix = builder.Create<EndsWith>("Tes").Not();
var rule = builder
    .Create<StartsWith>("ola")
    .Or(suffix);
```

`Not()` negates the complete current rule, whether it is a leaf or a composed group. This replaces the former `AndNot`, `OrNot`, and `XorNot` overload families.

Generic and runtime-type overloads are available:

```csharp
var rule = builder
    .Create(typeof(StartsWith), "Nik")
    .And(typeof(EndsWith), "sla");
```

## Use argument scope

Use `Argument.From<T>(...)` for a context-backed argument:

```csharp
var rule = builder.Create<StartsWith>(
    Argument.From<string>(scope => scope.GetVariable<string>("prefix")));

var context = EvaluationContext.CreateBuilder()
    .AddValue("prefix", "Nik")
    .Build();

var predicate = rule.Build().WithContext(context);
```

The argument scope is read-only and is created for each argument invocation. It exposes the current value, expression root, enclosing root, and variable lookup without mutating shared state.

## Reuse and render a rule

Rules are immutable. Combining or negating a rule leaves the earlier value unchanged. `Build()` is non-consuming and may be called repeatedly; `ToSource()` is available before or after building:

```csharp
var source = rule.ToSource();
var first = rule.Build();
var second = rule.Build();
```

Rules combined with `And(Rule)`, `Or(Rule)`, or `Xor(Rule)` must originate from the same builder/factory instance.
