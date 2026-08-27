---
title: Getting Started
nav_order: 1
permalink: /getting-started/
---

Expressif is a language for defining data transformations, validations, computations, and aggregations as expressions. It was initially developed as part of [nbi.io](https://www.nbi.io) to describe small transformations over scalar values, then grew into a language and function library useful beyond that original context.

## Why Expressif?

Expressif has three primary design goals.

### Readable expressions

Expressions should be easy to understand. Functions are applied from left to right, following the natural order in which the transformations are read.

### Predictable execution

Functions should very rarely throw exceptions.

For example, most languages throw an exception when attempting to retrieve the fifth character of a two-character string. Expressif instead returns `#null`, allowing the surrounding expression to continue safely.

### Portable across ecosystems

Expressions should not be tied to a single runtime or technology stack. The same language and behavior should be implementable across ecosystems such as .NET, Python, and DuckDB, allowing an expression to retain its meaning wherever it is evaluated.

## Where to continue

- Start with the [Expressif language](../language/) to learn how to read and write expressions.
- Use the [command-line interface](../cli/) to run Expressif from a terminal or automation workflow.
- Use the [.NET SDK](../dotnet-sdk/) to install and evaluate Expressif from C#.
- Browse the [function reference](../functions/), [predicate reference](../predicates/), and [accumulator reference](../accumulators/) for the available operations.
- See [Tooling](../tooling/) for editor support.
