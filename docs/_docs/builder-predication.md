---
title: Programmatically build a predication
subtitle: Build a predication with C# code
tags: [programmatically]
---

If you're familiar with building [expressions](builder-expression/), buidling predications without using the Expressif language is similar. To achive this, use the class `PredicationBuilder`. The first predicate can be specified with the help of the `Chain` method.

<!-- START INCLUDE "PredicationBuilderTest.cs/Chain_WithParameter_CorrectlyEvaluate" -->
```csharp
var builder = new PredicationBuilder().Create<StartsWith>("Nik");
var predicate = builder.Build();
Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->

You can specify the combinational operator between two predicates with the help of the `And`, `Or`, `Xor` methods.

<!-- START INCLUDE "PredicationBuilderTest.cs/AndOrXor_Generic_CorrectlyEvaluate" -->
```csharp
var builder = new PredicationBuilder()
    .Create<StartsWith>("ola")
    .Or<EndsWith>("sla")
    .And<SortedAfter>("Alan Turing")
    .Xor<SortedBefore>("Marie Curie");
var predicate = builder.Build();
Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->

To negate a predicate, you must use the `AndNot`, `OrNot` or `XorNot` methods from the builder class.

<!-- START INCLUDE "PredicationBuilderTest.cs/Chain_NegateGenericFluent_CorrectlyEvaluate" -->
```csharp
var builder = new PredicationBuilder()
    .Create<StartsWith>("ola")
    .OrNot<EndsWith>("Tes");
var predicate = builder.Build();
Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
```
<!-- END INCLUDE -->

Any subpredication is automatically assigned as group. The [serialization](../serializers/) is illustrating this behaviour

<!-- START INCLUDE "PredicationBuilderTest.cs/Serialize_SubPredication_CorrectlySerialized" -->
```csharp
var subPredicate = new PredicationBuilder()
    .Create<StartsWith>("Nik")
    .And<EndsWith>("sla");

var builder = new PredicationBuilder()
    .Create<LowerCase>()
    .Or(subPredicate)
    .Or<UpperCase>();

var str = builder.Serialize();
Assert.That(str, Is.EqualTo("{{lower-case |OR {starts-with(Nik) |AND ends-with(sla)}} |OR upper-case}"));
```
<!-- END INCLUDE -->

