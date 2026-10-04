---
layout: docs
title: Evaluate an expression
parent: .NET SDK
nav_order: 20
description: Evaluate Expressif expressions from C# and supply input and context values.
---

Create an expression from its Expressif source, then pass the value to transform to `Evaluate(...)`:

```csharp
var environment = ExpressifEnvironment.Default;
var normalizeName = environment.CreateExpression("trim | upper");

var firstResult = normalizeName.Evaluate("  Nikola Tesla  ");
var secondResult = normalizeName.Evaluate("  Ada Lovelace  ");
```

`CreateExpression(...)` parses the source against the environment's registered libraries and returns an executable `IExpression`. Parsing happens only once; the same object can then transform any number of values. Here, `firstResult` is `"NIKOLA TESLA"` and `secondResult` is `"ADA LOVELACE"`.

The two calls play different roles:

- `environment.CreateExpression("trim | upper")` defines what to do and which library snapshot supplies the operators.
- `Evaluate("  Nikola Tesla  ")` supplies the value on which to do it.

`Evaluate(...)` accepts any supported .NET value and returns `object?` because different expressions can produce different types. Check or cast that result when your C# code needs a specific type:

```csharp
if (firstResult is string normalizedName)
    Console.WriteLine(normalizedName);
```

See [Expressions](../../language/expressions/) for the Expressif pipeline and function-call syntax.

## Supply variables

Variables hold values that are not part of the input itself, such as a user preference or application setting. Create the expression first, then attach an `EvaluationContext` containing those values:

```csharp
var expression = environment.CreateExpression("append(@suffix)");

var context = new EvaluationContext(
    new Dictionary<string, object?>
    {
        ["suffix"] = " Nikola!"
    }
);

var configuredExpression = expression.WithContext(context);
var result = configuredExpression.Evaluate("Hello");
```

`result` is `"Hello Nikola!"`. The dictionary stores the name as `suffix`; the Expressif source refers to it as `@suffix`.

`WithContext(...)` returns a new expression. It does not modify the original one. This lets the application reuse one parsed expression with different immutable contexts:

```csharp
var expression = environment.CreateExpression("append(@suffix)");

var excited = expression.WithContext(new EvaluationContext(
    new Dictionary<string, object?> { ["suffix"] = "!" }
));

var questioning = expression.WithContext(new EvaluationContext(
    new Dictionary<string, object?> { ["suffix"] = "?" }
));

var first = excited.Evaluate("Really");       // "Really!"
var second = questioning.Evaluate("Really"); // "Really?"
```

`EvaluationContext` copies the supplied variables and exposes them as a read-only dictionary. An expression configured this way can be evaluated concurrently.

`Context` has a different lifetime. Pass it to `CreateExpression(...)`, `CreateExpressionBuilder(...)`, or `CreatePredicationBuilder(...)` when binding or builder parameter delegates need host-supplied values. Use `EvaluationContext` with `WithContext(...)` for immutable variables attached to an already-created executable object. Keeping those roles separate lets one environment create many independently configured expressions.

## Evaluate structured .NET values

Pass a structured value directly to `Evaluate(...)`. Expressif can read fields from dictionaries and supported .NET objects:

```csharp
var formatName = environment.CreateExpression(".name | trim | append(^.suffix)");

var input = new Dictionary<string, object?>
{
    ["name"] = "Ada Lovelace  ",
    ["suffix"] = " (mathematician)"
};

var result = formatName.Evaluate(input);
```

`result` is `"Ada Lovelace (mathematician)"`. For this outer expression, the input passed to `Evaluate(...)` is the expression root, so `^.suffix` still refers to that record after `.name` changes the value flowing through the pipeline. When a function invokes a nested expression, the value passed to the nested expression becomes its own root.

Each call receives its own evaluation frame. The same expression can therefore process several inputs—including concurrent inputs—without one call replacing another call's expression root.

## Format structured results

`ValueFormatter` renders evaluation results with Expressif scalar and structured-value
syntax. Compact output is the default. Request pretty output when inspecting nested
values, including from the Visual Studio Watch or Immediate window:

```csharp
var compact = ValueFormatter.Format(result);
var pretty = ValueFormatter.Format(result, ValueFormat.Pretty);
var fourSpaces = ValueFormatter.Format(result, ValueFormat.Pretty, "    ");
```

Pretty output writes each structured element on its own line and uses two spaces per
nesting level. Empty structures remain on one line. Formatting does not change the
underlying result.

Use `ValueFormattingOptions` for hybrid pretty output. `InlineValueTypes` contains
the exact runtime types that may remain inline, and `PreferredLineWidth` includes
the indentation and other text already written on the current line:

```csharp
var hybrid = ValueFormatter.Format(result, new ValueFormattingOptions
{
    Format = ValueFormat.Pretty,
    InlineValueTypes = new HashSet<Type> { typeof(TupleValue) },
    PreferredLineWidth = 100,
});
```

For each eligible value, the formatter measures the complete compact subtree. If it
fits, the subtree is emitted atomically on one line. If it does not, that value uses
the normal pretty layout and eligible descendants are considered independently.
Compact formatting ignores the inline policy. Leaving `InlineValueTypes` empty
preserves the standard pretty output.

See [References](../language/references.md) for field, variable, and expression-root syntax. See [Advanced expressions](../language/advanced.md) for nested expressions and other language features.
# Function observation

An expression observer creates one observation for each parse, bind, or evaluation operation. Return
`null` for stages that are not supported. To inspect function boundaries, make the evaluation
observation implement `IFunctionObserver`:

```csharp
sealed class FunctionMetricsObserver : IExpressionObserver
{
    public IExpressionObservation? Create(ExpressionObservationStage stage)
        => stage == ExpressionObservationStage.Evaluate ? new Evaluation() : null;

    private sealed class Evaluation : IExpressionObservation, IFunctionObserver
    {
        public void OnCompleted(FunctionObservationContext function, object? input, object? output)
            => Console.WriteLine($"{function.Id}: {function.CanonicalName} completed");

        public void OnFailed(FunctionObservationContext function, object? input, Exception exception)
            => Console.WriteLine($"{function.Id}: {function.CanonicalName} failed");

        public void Complete() { }
        public void Fail(Exception exception) { }
        public void Dispose() { }
    }
}

var observer = new FunctionMetricsObserver();
var factory = environment.CreateExpressionFactory(observer: observer);
var expression = factory.Create("trim | upper");
var result = expression.Evaluate("  hello  ");
```

`OnCompleted` runs after a function returns and `OnFailed` after it throws.
`FunctionObservationContext.Id` distinguishes repeated and nested bound nodes, while `Function`
contains the canonical operator identity. `ExpressionFactory` automatically activates the evaluation
observation while `Evaluate` is running.

Use `ExpressionObservers.Combine(first, second)` to combine observers. Lifecycle and detail callbacks
run in registration order; observations are disposed in reverse order. Combining no observers returns
`null`, and combining one returns that observer unchanged.

Hosts that own a wider operation can create and activate an observation directly. Keep both the
observation and activation alive until deferred results have been enumerated and output has been
written:

```csharp
using var observation = observer.Create(ExpressionObservationStage.Evaluate);
using var activation = observation?.Activate();

try
{
    var result = expression.Evaluate(input);
    WriteResult(result); // Include deferred enumeration in the observation.
    observation?.Complete();
}
catch (Exception exception)
{
    observation?.Fail(exception);
    throw;
}
```

Activations are scoped and nest safely: disposing an inner activation restores the outer observation.
The host owns completion, failure, disposal, and the lifetime of lazy results. An activation never
completes or disposes its observation.

Observers are passive: they must not mutate the input or output references they receive. They can
see raw values, so implementations are responsible for thread safety and for protecting, retaining,
and disposing of sensitive data. Each function is invoked exactly once, observer exceptions are
isolated from the authoritative result or failure, and lazy results are never enumerated merely for
observation. When no active observation exposes a detail capability, evaluation takes the direct path.
