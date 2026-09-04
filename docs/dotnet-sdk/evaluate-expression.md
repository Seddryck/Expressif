---
layout: docs
title: Evaluate an expression
parent: .NET SDK
nav_order: 20
description: Evaluate Expressif expressions from C# and supply input and context values.
---

Create an expression from its Expressif source, then pass the value to transform to `Evaluate(...)`:

```csharp
var normalizeName = Expression.Create("trim | upper");

var firstResult = normalizeName.Evaluate("  Nikola Tesla  ");
var secondResult = normalizeName.Evaluate("  Ada Lovelace  ");
```

`Expression.Create(...)` parses the source and returns an executable `IExpression`. Parsing happens only once; the same object can then transform any number of values. Here, `firstResult` is `"NIKOLA TESLA"` and `secondResult` is `"ADA LOVELACE"`.

The two calls play different roles:

- `Expression.Create("trim | upper")` defines what to do.
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
var expression = Expression.Create("suffix(@suffix)");

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
var expression = Expression.Create("suffix(@suffix)");

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

## Evaluate structured .NET values

Pass a structured value directly to `Evaluate(...)`. Expressif can read fields from dictionaries and supported .NET objects:

```csharp
var formatName = Expression.Create(".name | trim | suffix(^.suffix)");

var input = new Dictionary<string, object?>
{
    ["name"] = "Ada Lovelace  ",
    ["suffix"] = " (mathematician)"
};

var result = formatName.Evaluate(input);
```

`result` is `"Ada Lovelace (mathematician)"`. For this outer expression, the input passed to `Evaluate(...)` is the expression root, so `^.suffix` still refers to that record after `.name` changes the value flowing through the pipeline. When a function invokes a nested expression, the value passed to the nested expression becomes its own root.

Each call receives its own evaluation frame. The same expression can therefore process several inputs—including concurrent inputs—without one call replacing another call's expression root.

See [References](../language/references.md) for field, variable, and expression-root syntax. See [Advanced expressions](../language/advanced.md) for nested expressions and other language features.
