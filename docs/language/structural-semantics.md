---
title: Structural semantics
parent: Expressif language
nav_order: 6.6
description: Machine-readable relationships between the values an operator visits and the values it returns.
---

Structural semantics describe how an operator's output relates to the values it visits. They complement input and output types, traversal, and parameter evaluation; they do not replace them.

Catalog entries omit `Semantics` when no structural collection relationship needs to be exposed. When the relationship matters but a dimension cannot be stated more precisely, that dimension uses `unknown`. Consumers must not infer structural semantics from an operator's name, scope, input type, output type, summary, or behavior prose.

## Cardinality

| Value | Meaning |
|:------|:--------|
| `preserved` | The output contains the same number of elements as the visited input. |
| `non-increasing` | The output contains no more elements than the visited input. |
| `collapsed` | The visited collection produces one result. |
| `expanded` | One visited input can produce multiple output elements. |
| `partitioned` | Visited inputs are reorganized into groups or partitions. |
| `unknown` | Cardinality is structurally relevant but cannot be declared more precisely. |

## Dependency

| Value | Meaning |
|:------|:--------|
| `per-element` | An output element depends only on its corresponding visited input element. |
| `prefix` | An output at a position depends on the visited prefix ending at that position. |
| `whole-input` | An output depends on the complete visited input. |
| `partition` | An output depends on the elements belonging to the same partition or key. |
| `unknown` | Dependency is structurally relevant but cannot be declared more precisely. |

## Ordering

| Value | Meaning |
|:------|:--------|
| `preserved` | Relative source order is retained. |
| `reordered` | The operator deliberately changes relative order. |
| `unordered` | Output order is not part of the semantic contract. |
| `not-applicable` | The result has no element ordering to describe. |
| `unknown` | Ordering is structurally relevant but cannot be declared more precisely. |

## Read semantics, traversal, and evaluation separately

Traversal identifies which values an operator visits. Parameter evaluation identifies when an argument expression runs and which context it receives. Structural semantics identify the relationship between visited input and output.

For example, `map` and `broadcast` both visit every incoming array element and preserve cardinality and order. A mapped output depends only on its corresponding input element, while every broadcast output depends on the complete input. Their dependency values are therefore `per-element` and `whole-input`, respectively.

`filter` also visits each incoming element, but its cardinality is `non-increasing` because a visited element can be omitted. `fold` collapses the visited collection to one whole-input-dependent result, so ordering is `not-applicable`. `scan` preserves cardinality and order while each intermediate result depends on a prefix. `group-by` partitions the input and each resulting group depends on the elements assigned to that partition.
