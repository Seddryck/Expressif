---
layout: docs
title: "scan"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 160
has_toc: false
permalink: /functions/array/aggregation/scan/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array<T> →
scan(
    accumulator: accumulator
) → array<U>
```

Executes an accumulator progressively over the input enumerable and returns the intermediate accumulated value after each input element. Preserves input cardinality (one output item per input item). This differs from fold (final value only) and broadcast (final value repeated). Returns `null` when the input is not an enumerable or is a string.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `accumulator` | `accumulator` | Yes | Factory that creates the accumulator instance used for the scan execution. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<U>`
- **`accumulator`:** Receives `T` and returns `U`.
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `prefix` <span class="semantics-info" title="An output at a position depends on the visited prefix ending at that position." aria-label="Dependency definition: An output at a position depends on the visited prefix ending at that position.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array entering this call.

- **`accumulator`:** The selected accumulator receives each incoming array element through its accumulation lifecycle; the accumulator factory is not recreated for each element.



## Examples

{% raw %}
```expressif
{1, 2, 3} | scan(sum) → {1, 3, 6}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** `array-to-scan`
{: .member-reference }
