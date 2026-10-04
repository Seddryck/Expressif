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
| `.Chain<T>()` and consuming expression builders | `.Create<T>().Then<T>()`; pipelines are persistent and reusable. |
| `AndNot<T>()`, `OrNot<T>()`, or `XorNot<T>()` | Compose a rule and call `Not()` on that leaf or group. |
| Mutable `Context` variables | Build an immutable `EvaluationContext` and attach it with `WithContext(...)`. |
| Context-backed builder delegates | `Argument.From<T>(ArgumentEvaluationContext => ...)` |
| `Expressif.Values.Types` | `Expressif.Types` |
| `Pair`, `Tuple`, `Vector`, `Dictionary`, `Group`, `Grouping` runtime types | `PairValue`, `TupleValue`, `VectorValue`, `DictionaryValue`, `GroupValue`, `GroupingValue` |
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

The Core static methods `Expression.Create(text, binder)` and `Expression.CreateClosed(text, binder)` are removed. They looked like application conveniences but could not select the official Library vocabulary from Core. Normal applications use `ExpressifEnvironment`. A Core-only or custom host explicitly owns its binder:

```csharp
IExpressionBinder binder = CreateCustomBinder();
var expressions = new ExpressionFactory(binder);
var expression = expressions.Create("trim | upper");
var closed = expressions.CreateClosed("42 | increment");
```

No implicit assembly scanning or global mutable factory is involved. Applications can also obtain the environment-owned lower-level objects through `CreateExpressionBinder()`, `CreateExpressionFactory()`, and `CreateFunctionFactory()`.

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
    .Create<Lower>()
    .Then<FirstChars>(5)
    .Build();
```

The equivalent predication builder is also environment-owned:

```csharp
using Expressif.Library.Text;

var predication = ExpressifEnvironment.Default.CreatePredicationBuilder()
    .Create<StartsWith>("Nik")
    .Build();
```

The starter cannot build an empty pipeline or rule. `Create(...)` returns the valid nested state. Pipelines and rules are immutable: composition returns a new value, `Build()` is non-consuming, and `ToSource()` works before or after repeated builds when every argument can be represented as Expressif source.

## Separate evaluation values from argument scope

`EvaluationContext` is the only public source of host-provided variables referenced by textual expressions with `@name`. Build an immutable registration snapshot and attach it to an executable expression or predication:

```csharp
var expression = ExpressifEnvironment.Default
    .CreateExpression("suffix(@suffix)")
    .WithContext(EvaluationContext.CreateBuilder()
        .AddValue("suffix", "!")
        .Build());

var result = expression.Evaluate("Hello"); // "Hello!"
```

Register dynamic host data with `AddProvider(...)`. Providers run in registration order. Each provider runs exactly once at the start of a top-level evaluation, receives its top-level input, and is materialized for nested and deferred work. A context can be reused concurrently, so provider delegates must be thread-safe:

```csharp
var context = EvaluationContext.CreateBuilder()
    .AddProvider("current-date", start => clock.Today)
    .Build();
```

Builder arguments use a separate immutable invocation scope:

```csharp
var builder = ExpressifEnvironment.Default.CreatePredicationBuilder();

var predication = builder
    .Create<StartsWith>(Argument.From<string>(scope =>
        scope.GetVariable<string>("prefix")))
    .Build();
```

`ArgumentEvaluationContext` exposes `Current`, `Root`, `EnclosingRoot`, and read-only variable lookup. Its provider follows the operator's documented argument-evaluation frequency. Because that provider is executable host code, a pipeline or rule containing it cannot be rendered by `ToSource()`. The mutable `Context`, `IContext`, `ContextVariables`, `ContextObject`, and `ContextParameter` hierarchy is no longer public.

## Update public model and type names

All type-authoring APIs now live under one namespace:

```csharp
using Expressif.Types;
```

This includes `ExpressifTypeAttribute`, `ExpressifTypeDefinition<T>`, descriptors, registries, quoted-literal parsers, and their exceptions. Semantic type names and catalog/JSON formats do not change.

Runtime value implementations now use explicit names that avoid BCL collisions: `PairValue`, `TupleValue`, `VectorValue`, `DictionaryValue`, `GroupValue`, and `GroupingValue`. The shorter public CLR type names are removed; no parallel hierarchy or forwarding types are retained.

Binding, logical-plan, and logical-schema collection properties now snapshot their inputs. Mutating a caller-owned list or dictionary after construction cannot change a model. Collection-bearing records use structural equality and compatible hash codes, while existing logical-plan and schema JSON shapes remain compatible.

External binders construct canonical calls through `Function.FromArguments(...)` or the positional convenience `Function.FromParameters(...)`. `FunctionArgument` preserves names and spread markers. `FunctionSyntax` is no longer public; source notation and runtime roles are internal metadata.

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
4. Replace `Context` variables with `EvaluationContext` values/providers and builder delegates with `Argument.From<T>(...)`.
5. Build and run the application against each target framework it supports.

See [Load runtime libraries](runtime-libraries.md) for library manifests and compatibility checks, and [Evaluate an expression](evaluate-expression.md) for expression reuse and runtime contexts.
