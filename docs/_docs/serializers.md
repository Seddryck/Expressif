---
title: Serialize an expression or a predication
subtitle: Persist your ideas
tags: [programmatically]
---
You can also serialize an expression or a predication designed with an `ExpressionBuilder` or a `PredicationBuilder` into the Expressif language. 
To achieve this, use the method `Serialize` of the class `ExpressionBuilder` .

<!-- START INCLUDE "ExpressionBuilderTest.cs/Serialize_WithParameters_CorrectlySerialized" -->
```csharp
var builder = new ExpressionBuilder()
    .Chain<Lower>()
    .Chain<FirstChars>(5)
    .Chain<PadRight>(7, '*');
var str = builder.Serialize();
Assert.That(str, Is.EqualTo("lower | first-chars(5) | pad-right(7, *)"));
```
<!-- END INCLUDE -->

or the same method from the class `PredicationBuilder`

<!-- START INCLUDE "PredicationBuilderTest.cs/Serialize_Negate_CorrectlySerialized" -->
```csharp
var builder = new PredicationBuilder()
    .Create<StartsWith>("ola")
    .OrNot<EndsWith>("sla");
var str = builder.Serialize();
Assert.That(str, Is.EqualTo("{starts-with(ola) |OR !{ends-with(sla)}}"));
```
<!-- END INCLUDE -->
Both methods `Serialize` returns a textual representation of the expression/predicate. 
Take note that you don't need to build the expresion/predication before serializing it.
