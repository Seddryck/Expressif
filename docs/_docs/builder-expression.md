---
title: Programmatically build an expression
subtitle: Build an expression with C# code
tags: [programmatically]
---

On top of the existing Expressif language, you can also build expressions directly from C# code. To achieve this, you should use the class `ExpressionBuilder`. This class has a method `Chain` to let you define the functions composing your expression. Each call to the method Chain lets you add a function to the expression. Once all the functions have been added, you must call the method `build` to retrieve your expression. This expression can be used as any other expression by calling the `Evaluate` method.

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_MultipleWithoutParameters_CorrectlyEvaluate" -->
```csharp
var builder = new ExpressionBuilder()
    .Chain<Lower>()
    .Chain<Length>();
var expression = builder.Build();
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo(12));
```
<!-- END INCLUDE -->

For the functions requiring one or more parameters, you can specify them as arguments of the function `Chain`. 

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_WithParameters_CorrectlyEvaluate" -->
```csharp
var builder = new ExpressionBuilder().Chain<PadRight>(15, '*');
var expression = builder.Build();
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikola Tesla***"));
```
<!-- END INCLUDE -->

You can use the context values as parameters of the function. 

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_MultipleWithContext_CorrectlyEvaluate" -->
```csharp
var context = new Context();
var builder = new ExpressionBuilder(context)
    .Chain<Lower>()
    .Chain<PadRight>(ctx => ctx.Variables["myVar"], ctx => ctx.CurrentObject[1]);
var expression = builder.Build();

context.Variables.Add<int>("myVar", 15);
context.CurrentObject.Set(new List<char>() { '-', '*', ' ' });
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla***"));

context.Variables.Set("myVar", 16);
context.CurrentObject.Set(new List<char>() { '*', '+' });
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla++++"));
```
<!-- END INCLUDE -->

You can build your final expression by combining other expressions. Use also the function `Chain` with an expression as parameter. Using subexpression to define a parameter of a function is currently not supported.

<!-- START INCLUDE "ExpressionBuilderTest.cs/Chain_SubExpression_CorrectlyEvaluate" -->
```csharp
var subExpressionBuilder = new ExpressionBuilder()
    .Chain<FirstChars>(5)
    .Chain<PadRight>(7, '*');

var builder = new ExpressionBuilder()
    .Chain<Lower>()
    .Chain(subExpressionBuilder)
    .Chain<Upper>();

var expression = builder.Build();
Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("NIKOL**"));
```
<!-- END INCLUDE -->

If you don't like generics (or if you cannot use them), you can also use the override of the method `Chain` accepting types.

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

