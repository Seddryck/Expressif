---
layout: docs
title: Migrate from v2 to v3
parent: .NET SDK
nav_order: 90
description: Move a .NET application from the v2 static factories and constructors to the v3 environment composition model.
---

Version 3 makes `ExpressifEnvironment` the composition root for the .NET SDK. An environment is an immutable snapshot of the operator libraries, types, catalogs, and literal parsers available to an application. Create textual expressions, textual predications, and typed builders from the same environment so they resolve the same vocabulary.

## Select packages

Most applications should reference the `Expressif` umbrella package. It supplies both `Expressif.Core` and the official `Expressif.Library` vocabulary:

```bash
dotnet add package Expressif
```

Reference `Expressif.Core` alone only when building a host that deliberately supplies a different vocabulary. A normal application should not construct type sources, registries, binders, or function factories itself.

## Replace static creation

Create or retain an environment, then use it for every composition path:

```csharp
using Expressif.Hosting;

var environment = ExpressifEnvironment.Default;
var expression = environment.CreateExpression("trim | upper");
var predication = environment.CreatePredication("lower-case");

var result = expression.Evaluate("  Alice  "); // "ALICE"
var valid = predication.Evaluate("alice");     // true
```

The common replacements are:

| v2 pattern | v3 pattern |
|:--|:--|
| `Expression.Create(source)` | `environment.CreateExpression(source)` |
| `Predication.Create(source)` | `environment.CreatePredication(source)` |
| direct `ExpressionBuilder` construction | `environment.CreateExpressionBuilder()` |
| direct `PredicationBuilder` construction | `environment.CreatePredicationBuilder()` |
| manual `ExpressionBinder`, `FunctionFactory`, or type-source wiring | `environment.CreateExpressionFactory()` or another environment helper |

The old convenience APIs and parameterless builder construction are not the v3 composition contract. Moving creation to the environment also prevents a textual expression and a typed builder from silently using different library sets.

## Migrate typed builders

Create builders from the same environment as textual expressions:

```csharp
var expression = environment.CreateExpressionBuilder()
    .Chain<Lower>()
    .Chain<FirstChars>(5)
    .Build();

var predicate = environment.CreatePredicationBuilder()
    .Create<StartsWith>("Nik")
    .Build();
```

Builders remain single-purpose composition objects. Reuse the built expression or predicate; create a new builder when composing another pipeline.

## Distinguish the two contexts

`Context` belongs to composition. Pass one to an environment helper when builder parameter delegates or binding need host values:

```csharp
var context = new Context();
context.Variables.Add<string>("suffix", "!");

var function = environment.CreateExpressionBuilder(context)
    .Chain<Append>(ctx => ctx.Variables["suffix"])
    .Build();
```

`EvaluationContext` belongs to an executable expression or predication. `WithContext(...)` returns a new wrapper and leaves the reusable original unchanged:

```csharp
var expression = environment.CreateExpression("append(@suffix)");

var excited = expression.WithContext(new EvaluationContext(
    new Dictionary<string, object?> { ["suffix"] = "!" }));
var questioning = expression.WithContext(new EvaluationContext(
    new Dictionary<string, object?> { ["suffix"] = "?" }));
```

Use `Context` while composing and `EvaluationContext` while configuring repeated evaluations. Do not mutate shared composition context as a substitute for per-evaluation state.

## Register extension libraries

Install the extension package, register its marker type, and keep using the returned environment:

```csharp
using Expressif.Library.SemVer;

var environment = ExpressifEnvironment.Default
    .RegisterLibrary<SemVerLibrary>();

var expression = environment.CreateClosedExpression(
    "#\"1.2.3\":semver | bump-patch");
```

`RegisterLibrary(...)` does not mutate `ExpressifEnvironment.Default`. It validates the supplied assembly and returns a new snapshot. Existing environments and expressions retain their original capabilities, and Expressif does not scan ambient assemblies for extensions.

## Reuse the environment and executable objects

An environment is safe to retain as an application-level dependency. Creating an expression parses and binds its source once; retain that executable object for repeated evaluation when its rule is stable. Derive another environment only when a host needs a different registered library set.

If an application previously exposed Core implementation details such as concrete type sources or binder internals, replace that wiring with the environment methods before removing the obsolete references. See [Load runtime libraries](runtime-libraries.md) for the extension contract and [Evaluate an expression](evaluate-expression.md) for runtime context examples.
