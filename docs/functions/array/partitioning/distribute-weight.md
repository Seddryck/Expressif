---
layout: docs
title: "distribute-weight"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 80
has_toc: false
permalink: /functions/array/partitioning/distribute-weight/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array<T> →
distribute-weight(
    weight: expression
) → array<array<T>>
```

Distributes array values into two groups whose aggregate evaluated weights are approximately balanced. Returns `null` when the input or a weight cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weight` | `expression` | Yes | Specifies the expression that produces a finite, non-negative numeric weight for each input value. |



## Value shape

- Pipeline input: `array<T>`
- Returns: `array<array<T>>`
- `weight`: Receives `T` and returns `numeric`.
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array entering this call.

- **`weight`:** Evaluated once per visited element, with that element as its context.


## Behavior

`distribute-weight` evaluates `weight` exactly once per input value, orders values by descending weight using original position to break equal-weight ties, and assigns each value to the group with the lower aggregate weight. Aggregate-weight ties prefer the group with fewer values; remaining ties prefer the first group. It then restores relative input order within both groups. This deterministic largest-weight-first strategy is best-effort and does not guarantee the mathematically optimal partition. Empty input returns two empty arrays and a singleton is placed in the first group.



## Examples

{% raw %}
```expressif
{8, 7, 6, 5} | distribute-weight(neutral) → {{8, 5}, {7, 6}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
