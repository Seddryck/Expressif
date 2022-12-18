---
title: Why did we create Expressif?
subtitle: The impossible quest of building database-vendors agnostic solutions in .NET
tags: [quick-start]
---
We started the development of Expressif within [nbi.io](https://www.nbi.io) to support the definition of a few tiny transformations through functions to some scalar values. Overtime, the list of supported functions grow up and the possibility to introduce some variables into the expression was introduced. The possibility to re-use this development outside of this initial became clear.

Expressif's primary design goals are to:

* Easy-to-read definition of expressions: functions are applied from left to right
* Always successful implementation of functions: we believe that functions should extremely rarely throw exceptions. i.e, if you try to retrieve the fifth char of a text value containing only two chars, most of the languages will throw an exception but not Expressif. Expressif will simply returns null.
* The primary goal of the functions and predicates library is to be consumed by the parsing engine but it should also be usable in other contexts, i.e. direct calls to functions in C# code.