---
title: First steps with a predication
subtitle: How to quickly define a predication and evaluate it.
tags: [quick-start]
---

Expressif provides a class named *Predication* to define a combination of predicates applied to an argument. The class is expecting the textual representation of the predicates in its constructor.

<!-- START INCLUDE "PredicationTest.cs/Evaluate_SinglePredicateWithoutParameter_Valid" -->
```csharp
var predication = new Predication("lower-case");
var result = predication.Evaluate("Nikola Tesla");
Assert.That(result, Is.False);
```
<!-- END INCLUDE -->

Same than for expressions, some predicates require parameters, you can specify them between the parenthesis immediately following the predicate name. More specifically, some predicates require an interval as parameter. The parameter can be define with the help of square brackets or parenthesis.

<!-- START INCLUDE "PredicationTest.cs/Evaluate_IntervalAsParameter_Valid" -->
```csharp
var predication = new Predication("within-interval([0;20[)");
var result = predication.Evaluate(15);
Assert.That(result, Is.True);
```
<!-- END INCLUDE -->

Other predicates require a culture as parameter. To specify a culture just use the textual representation of the culture composed of the two letter ISO code of the language then the two letters ISO code of the country separated by a dash i.e. `fr-be` for Belgian French, `nl-be` for Belgian Dutch or `de-de` for German.

<!-- START INCLUDE "PredicationTest.cs/Evaluate_CultureAsParameter_Valid" -->
```csharp
var predication = new Predication("matches-date(fr-fr)");
var result = predication.Evaluate("28/12/1978");
Assert.That(result, Is.True);
```
<!-- END INCLUDE -->

Any predicate can be negated to return the opposite result. To negate a predicate just put the exclamation mark (`!`) in front of the predicate name.

<!-- START INCLUDE "PredicationTest.cs/Evaluate_Negation_Valid" -->
```csharp
var predication = new Predication("!starts-with(Nik)");
var result = predication.Evaluate("Nikola Tesla");
Assert.That(result, Is.False);
```
<!-- END INCLUDE -->

You can combine the predicates. Each predicate will accept the same argument and will be evaluated separatly. The results of the combination is dependening on the combinational operator used. To specify the name of the combinational operator use the pipe operator (`|`) immediately followed by the name of the operator. The following operators are valid `|AND`, `|OR`, `|XOR`. 

Take into account that when possible, the operators are implementing a short-circuit. If the two predicates are combined with the operator `|AND` and the first is returning `false`, the second will not be evaluated. Following the same reasoning, if the two predicates are combined with the operator `|OR` and the first is returning `true`, the second will also be ignored.

<!-- START INCLUDE "PredicationTest.cs/Evaluate_CombinationAnd_Valid" -->
```csharp
var predication = new Predication("starts-with(Nik) |AND ends-with(sla)");
var result = predication.Evaluate("Nikola Tesla");
Assert.That(result, Is.True);
```
<!-- END INCLUDE -->

By default, the predicates are combined from left to right. If you've three predicates, the two firsts will be combined and then the result of this combination will be combined with the third predicate. To alter this order, you must group the predicates with the help of curly braces `{...}`. Each predicate inside a group is evaluated from left to right and then the result of the group is combined with another group or predicate also from left to right.

<!-- START INCLUDE "PredicationTest.cs/Evaluate_CombinationsGroup_Valid" -->
```csharp
var predication = new Predication("{starts-with(Nik) |AND ends-with(sla)} |OR {starts-with(ola) |AND ends-with(Tes)}");
var result = predication.Evaluate("Nikola Tesla");
Assert.That(result, Is.True);

var withoutGroupsPredication = new Predication("starts-with(Nik) |AND ends-with(sla) |OR starts-with(ola) |AND ends-with(Tes)");
var secondResult = withoutGroupsPredication.Evaluate("Nikola Tesla");
Assert.That(result, Is.Not.EqualTo(secondResult));
```
<!-- END INCLUDE -->
