---
title: First steps with an expression
subtitle: How to quickly define an expression and evaluate it.
tags: [quick-start]
---

Expressif provides a class named *Expression* to define a chain of functions applied to a value. The class is expecting the textual representation of the chained functions in its constructor.

<!-- START INCLUDE "ExpressionTest.cs/Evaluate_SingleFunctionWithoutParameter_Valid" -->
```csharp
var expression = new Expression("lower");
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola tesla"));
```
<!-- END INCLUDE -->

Some functions require parameters, you can specify them between the parenthesis following the function name. Note that literal textual values don't required quotes surronding them.

<!-- START INCLUDE "ExpressionTest.cs/Evaluate_SingleFunctionWithOneParameter_Valid" -->
```csharp
var expression = new Expression("remove-chars(a)");
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("Nikol Tesl"));
```
<!-- END INCLUDE -->

You can chain the functions with the operator pipe (`|`). The functions are executed from left to right.

<!-- START INCLUDE "ExpressionTest.cs/Evaluate_TwoFunctions_Valid" -->
```csharp
var expression = new Expression("lower | remove-chars(a)");
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikol tesl"));
```
<!-- END INCLUDE -->

It's possible to use variables as function parameters. The name of the variables must always start by an arobas (`@`)

<!-- START INCLUDE "ExpressionTest.cs/Evaluate_VariableAsParameter_Valid" -->
```csharp
var context = new Context();
context.Variables.Add<char>("myChar", 'k');

var expression = new Expression("lower | remove-chars(@myChar)", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("niola tesla"));
```
<!-- END INCLUDE -->

In addition to the variables that must be scalar values (text, numeric, dateTime ...), you can also add a property-object to the context. A property-object must be a pure C# object, an IDictionnary, an IList, or a DataRow. You can access the properties of the property-object based on the property's name with the syntax `[property-name]`.

<!-- START INCLUDE "ExpressionTest.cs/Evaluate_ObjectPropertyAsParameter_Valid" -->
```csharp
var context = new Context();
context.CurrentObject.Set(new { CharToBeRemoved = 't' });

var expression = new Expression("lower | remove-chars([CharToBeRemoved])", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola esla"));
```
<!-- END INCLUDE -->

or based on its position with the syntax `#index` (where index is positive number).

<!-- START INCLUDE "ExpressionTest.cs/Evaluate_ObjectIndexAsParameter_Valid" -->
```csharp
var context = new Context();
context.CurrentObject.Set(new List<char>() { 'e', 's' });

var expression = new Expression("lower | remove-chars(#1)", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola tela"));
```
<!-- END INCLUDE -->

It's also possible to use the result of function as the value of a parameter for another function. To achieve this the function as a parameter must be surrounded by curly braces `{...}`.

<!-- START INCLUDE "ExpressionTest.cs/Evaluate_FunctionAsParameter_Valid" -->
```csharp
var context = new Context();
context.Variables.Add<int>("myVar", 6);
context.CurrentObject.Set(new List<int>() { 15, 8, 3 });

var expression = new Expression("lower | skip-last-chars( {@myVar | subtract(#2) })", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola te"));
```
<!-- END INCLUDE -->
