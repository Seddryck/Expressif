---
layout: docs
title: "group-by"
parent: "Grouping functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/grouping/group-by/
tags:
  - functions
  - array/grouping
generated: true
---

```
array<T> →
group-by(
    ...expressions: expression
) → grouping<K, T>
```

Groups input values by keys calculated from one or more expressions.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expressions` | `expression` | Variadic (one or more); no spread | One or more expressions evaluated once per input value; multiple results form a tuple key. |



## Examples

{% raw %}
```expressif
{"BE", "be", "FR"} | group-by(lower) → #{("be" => {"BE", "be"}), ("fr" => {"FR"})}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `grouping<K, T>`
- `expressions`: Receives `T` and returns `K`.
- Combination: When multiple values are supplied, their output types become tuple positions in declaration order.
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array entering this call.

- **`expressions`:** Each supplied expression is evaluated once per visited element, with that element as its context.

**Kind:** Function  
**Scope:** `array/grouping`  
**Aliases:** None
{: .member-reference }
