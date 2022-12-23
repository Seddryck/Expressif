---
title: Programmatically build a predication
subtitle: Build a predication with C# code
tags: [programmatically]
---

If you're familiar with building [expressions](builder-expression/), buidling predications without using the Expressif language is similar. To achive this, use the class `PredicationBuilder`. The first predicate can be specified with the help of the `Chain` method.

<!-- START INCLUDE "PredicationBuilderTest.cs/Chain_WithParameter_CorrectlyEvaluate" -->
```csharp
var builder = new PredicationBuilder().Chain<StartsWith>("Nik");
var predication = builder.Build();
Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->

You can specify the combinational operator between two predicates with the help of the `And`, `Or`, `Xor` methods.

<!-- START INCLUDE "PredicationBuilderTest.cs/AndOrXor_Generic_CorrectlyEvaluate" -->
```csharp
var builder = new PredicationBuilder()
    .Chain<StartsWith>("ola")
    .Or<EndsWith>("sla")
    .And<SortedAfter>("Alan Turing")
    .Xor<SortedBefore>("Marie Curie");
var predication = builder.Build();
Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->

To negate a predicate, you must specify the `NotOperator` as a parameter of the previously mentioned methods.

<!-- START INCLUDE "PredicationBuilderTest.cs/Chain_NegateGenericFluent_CorrectlyEvaluate" -->
```csharp
var builder = new PredicationBuilder()
    .Chain<StartsWith>("ola")
    .Or<NotOperator, EndsWith>("Tes");
var predication = builder.Build();
Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->

Any subpredication is automatically assigned as group. The serialization is illustrating this behaviour

<!-- START INCLUDE "PredicationBuilderTest.cs/Serialize_SubPredication_CorrectlySerialized" -->
```csharp
var subPredicationBuilder = new PredicationBuilder()
    .Chain<StartsWith>("Nik")
    .Chain<AndOperator, EndsWith>("sla"); ;

var builder = new PredicationBuilder()
    .Chain<LowerCase>()
    .Chain<OrOperator>(subPredicationBuilder)
    .Chain<OrOperator, UpperCase>();

var str = builder.Serialize();
Assert.That(str, Is.EqualTo("lower-case |OR { starts-with(Nik) |AND ends-with(sla) } |OR upper-case"));
```
<!-- END INCLUDE -->

If you prefer to avoid the usage of the methods `And`, `Or`, `Xor`, you can stay with the `Chain` method and specify the combinational operator as a generic parameter.

<!-- START INCLUDE "PredicationBuilderTest.cs/Chain_NegateGenericFluent_CorrectlyEvaluate" -->
```csharp
var builder = new PredicationBuilder()
    .Chain<StartsWith>("ola")
    .Or<NotOperator, EndsWith>("Tes");
var predication = builder.Build();
Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->

You can also do it without generics

<!-- START INCLUDE "PredicationBuilderTest.cs/Chain_NegateType_CorrectlyEvaluate -->
```csharp
var builder = new PredicationBuilder()
    .Chain(typeof(StartsWith), "ola")
    .Chain(typeof(OrOperator), typeof(NotOperator), typeof(EndsWith), "Tes");
var predication = builder.Build();
Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->
