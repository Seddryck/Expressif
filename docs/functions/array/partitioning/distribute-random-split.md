---
layout: docs
title: "distribute-random-split"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/partitioning/distribute-random-split/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array<T> →
distribute-random-split(
    weights: array,
    seed?: integer
) → array<array<T>>
```

Randomly distributes array values among output arrays according to relative output weights. Returns `null` when the input, weights, or seed cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weights` | `array` | Yes | Specifies a non-empty array of finite, non-negative output weights with a positive total. |
| `seed` | `integer` | No | Specifies an optional seed that makes assignments reproducible on the same runtime version. When omitted, the value is derived from the runtime's shared random-number generator. |



## Examples

{% raw %}
```expressif
{1, 2, 3, 4, 5} | distribute-random-split({1, 0}, 42) → {{1, 2, 3, 4, 5}, {}}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<array<T>>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`weights`:** Evaluated once in the enclosing context.
- **`seed`:** Evaluated once in the enclosing context.


## Behavior

The weights are normalized by their total and specify assignment probabilities rather than exact output cardinalities. Each input value is independently assigned to exactly one output array. The result contains one array per weight, including empty arrays, and preserves relative input order within each output. Reusing the same seed, input, and weights produces the same result on the same runtime version. Empty input still validates the weights and returns one empty array per valid weight.

**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
