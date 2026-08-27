---
layout: docs
title: Build an expression with C#
parent: .NET SDK
nav_order: 40
description: Compose an Expressif function pipeline programmatically with ExpressionBuilder.
---

There are two ways to create an expression from C#.

Use `ExpressionBuilder` when the transformation is part of the program itself. The C# code names each function and supplies its parameters:

```csharp
var expression = new ExpressionBuilder()
    .Chain<Lower>()
    .Chain<FirstChars>(5)
    .Build();

var result = expression.Evaluate("Nikola Tesla");
```

Use `Expression.Create(...)` when the transformation arrives as text—for example, from a configuration file, database, or user interface:

```csharp
var source = configuration["NameTransformation"];
var expression = Expression.Create(source);

var result = expression.Evaluate("Nikola Tesla");
```

If `NameTransformation` contains `lower | first-chars(5)`, both examples return `"nikol"`.

The practical difference is who defines the transformation:

- With `ExpressionBuilder`, the developer defines it in C# and changing it normally requires rebuilding the program.
- With `Expression.Create(...)`, the transformation is data and can change without changing the C# code.

See [Evaluate an expression](../evaluate-expression/) for the text-based API.

## Build a pipeline

Call `Chain<T>()` once for each function, then call `Build()`:

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_MultipleWithoutParameters_CorrectlyEvaluate" -->
```csharp
var builder = new ExpressionBuilder()
    .Chain<Lower>()
    .Chain<Length>();

var expression = builder.Build();
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo(12));
```
<!-- END INCLUDE -->

Each function receives the result of the preceding function.

## Pass literal parameters

Pass constructor parameters to `Chain<T>(...)`:

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_WithParameters_CorrectlyEvaluate" -->
```csharp
var builder = new ExpressionBuilder()
    .Chain<PadRight>(15, '*');

var expression = builder.Build();
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikola Tesla***"));
```
<!-- END INCLUDE -->

The builder resolves a compatible function constructor when it builds the pipeline. Invalid function types or parameter lists fail at build time.

## Compose builders

Chain another builder to insert its functions into the pipeline:

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_SubExpression_CorrectlyEvaluate" -->
```csharp
var middle = new ExpressionBuilder()
    .Chain<FirstChars>(5)
    .Chain<PadRight>(7, '*');

var builder = new ExpressionBuilder()
    .Chain<Lower>()
    .Chain(middle)
    .Chain<Upper>();

var expression = builder.Build();
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("NIKOL**"));
```
<!-- END INCLUDE -->

This flattens the child builder into the parent pipeline. It does not create a nested expression parameter.

## Select function types at runtime

When the function type is known only at runtime, use the non-generic overload:

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_NotGeneric_CorrectlyEvaluate" -->
```csharp
var builder = new ExpressionBuilder()
    .Chain(typeof(Lower))
    .Chain(typeof(FirstChars), 5)
    .Chain(typeof(PadRight), 7, '*');

var expression = builder.Build();
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
```
<!-- END INCLUDE -->

The type must implement `IFunction`.

## Treat a builder as single-use

{: .warning }
`ExpressionBuilder.Build()` consumes every function stored in the builder. After `Build()` returns, calling `Build()` or `Serialize()` on that builder throws `InvalidOperationException`. Never retain or reuse a builder after building the expression.

If you need both the Expressif source and the executable expression, serialize first and build last:

```csharp
var builder = new ExpressionBuilder()
    .Chain<Lower>()
    .Chain<Length>();

var source = builder.Serialize(); // The functions are still available.
var expression = builder.Build(); // The builder is now empty.
```

See [Serialize a builder](../serialization/) for more about this lifecycle.
