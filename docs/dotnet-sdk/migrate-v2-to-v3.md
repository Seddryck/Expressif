---
layout: docs
title: Migrate C# integrations from v2 to v3
parent: .NET SDK
nav_order: 90
permalink: /dotnet-sdk/migrate-v2-to-v3/
description: Update package references, expression creation, predications, builders, contexts, and extension discovery for Expressif v3.
---

Expressif v3 separates the portable Core runtime from the official function Library. C# applications now start from an `ExpressifEnvironment`, an immutable snapshot of the libraries available to that host. This keeps operator discovery deterministic and lets independent hosts use different registered libraries without process-wide mutable configuration.

## Migration summary

| v2 integration | v3 replacement |
|:--|:--|
| Install the monolithic `Expressif` package. | Continue installing the `Expressif` umbrella package for the standard language. It references matching `Expressif.Core` and `Expressif.Library` packages. |
| `Expression.Create(source)` | `ExpressifEnvironment.Default.CreateExpression(source)` |
| `Expression.CreateClosed(source)` | `ExpressifEnvironment.Default.CreateClosedExpression(source)` |
| `Predication.Create(source)` | `ExpressifEnvironment.Default.CreatePredication(source)` |
| `new ExpressionBuilder()` | `ExpressifEnvironment.Default.CreateExpressionBuilder()` |
| `new PredicationBuilder()` | `ExpressifEnvironment.Default.CreatePredicationBuilder()` |
| Mutable context values used as runtime variables | Attach an immutable `EvaluationContext` with `WithContext(...)`. |
| Implicit extension discovery | Register the library assembly with `ExpressifEnvironment.RegisterLibrary(...)`. |

## Select the package

Most applications should keep one package reference:

```bash
dotnet add package Expressif
```

The umbrella package contains no competing runtime implementation. It references `Expressif.Core` and the official `Expressif.Library` at the same version. Install only `Expressif.Core` when building a host that intentionally supplies its own vocabulary. Do not mix Core, Library, and umbrella package versions.

## Create expressions from an environment

In v2, the static convenience selected the built-in vocabulary implicitly:

```csharp
var expression = Expression.Create("trim | upper");
```

In v3, select the environment explicitly:

```csharp
using Expressif.Hosting;

var environment = ExpressifEnvironment.Default;
var expression = environment.CreateExpression("trim | upper");
var result = expression.Evaluate("  Alice  "); // "ALICE"
```

Use `CreateClosedExpression(...)` when the source must not depend on pipeline input. If an application creates many expressions from the same environment, reuse its expression factory:

```csharp
var expressions = ExpressifEnvironment.Default.CreateExpressionFactory();
var normalizeName = expressions.Create("trim | upper");
var normalizeCode = expressions.Create("trim | lower");
```

Advanced hosts can still create binders and factories explicitly through `CreateExpressionBinder()` and `CreateFunctionFactory()`. Prefer the higher-level environment methods unless the host is replacing a parser or attaching an observer.

## Create strongly typed predications

Replace the v2 static factory:

```csharp
var predication = Predication.Create("lower-case");
```

with the environment-owned factory:

```csharp
var predication = ExpressifEnvironment.Default.CreatePredication("lower-case");

bool first = predication.Evaluate("Nikola Tesla"); // false
bool second = predication.Evaluate("nikola tesla"); // true
```

`CreatePredication(...)` guarantees a `bool` result to C# callers. Evaluation throws `InvalidCastException` if the supplied Expressif source produces a non-Boolean value.

## Create builders

Core builders no longer choose the official function library implicitly. Create them from the same environment used for textual expressions:

```csharp
using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Selection;

var expression = ExpressifEnvironment.Default.CreateExpressionBuilder()
    .Chain<Lower>()
    .Chain<FirstChars>(5)
    .Build();
```

The equivalent predication builder is also environment-owned:

```csharp
using Expressif.Library.Text;

var predication = ExpressifEnvironment.Default.CreatePredicationBuilder()
    .Create<StartsWith>("Nik")
    .Build();
```

Builder lifecycle rules are unchanged: serialize an `ExpressionBuilder` before calling `Build()`, because building consumes its queued pipeline.

## Separate binding context from runtime context

V3 distinguishes two context roles.

`EvaluationContext` contains immutable runtime variables referenced by textual expressions with `@name`. Attach it to an executable expression or predication:

```csharp
var expression = ExpressifEnvironment.Default
    .CreateExpression("suffix(@suffix)")
    .WithContext(new EvaluationContext(
        new Dictionary<string, object?> { ["suffix"] = "!" }));

var result = expression.Evaluate("Hello"); // "Hello!"
```

`Context` remains the mutable input to C# builder parameter delegates. Pass it when creating the builder:

```csharp
var context = new Context();
context.Variables.Add<string>("prefix", "Nik");

var predication = ExpressifEnvironment.Default
    .CreatePredicationBuilder(context)
    .Create<StartsWith>(value => value.Variables["prefix"])
    .Build();
```

Do not use a mutable `Context` as shared runtime-variable storage for otherwise reusable expressions. Use separate immutable `EvaluationContext` instances and `WithContext(...)` instead.

## Register extension libraries

V2 integrations that depended on loaded-assembly scanning must register each independently released library. Registration returns a new environment; it does not modify the source environment:

```csharp
using Expressif.Library.SemVer;

var environment = ExpressifEnvironment.Default
    .RegisterLibrary<SemVerLibrary>();

var expression = environment.CreateClosedExpression(
    "#\"1.2.3\":semver | bump-patch");
```

Expressions, predications, function factories, and builders created from this environment all see the registered SemVer capabilities. Objects previously created from `ExpressifEnvironment.Default` keep the original built-in-only snapshot.

## Validate the migration

After updating the integration:

1. Confirm that all Expressif packages use the same version.
2. Replace every unsupported static or parameterless creation pattern listed above.
3. Register every extension library before creating factories or executable rules.
4. Verify whether each existing `Context` value belongs to binding-time builder configuration or runtime evaluation.
5. Build and run the application against each target framework it supports.

See [Load runtime libraries](runtime-libraries.md) for library manifests and compatibility checks, and [Evaluate an expression](evaluate-expression.md) for expression reuse and runtime contexts.
