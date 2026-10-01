---
layout: docs
title: "zip-cycle"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/combination/zip-cycle/
tags:
  - functions
  - array/combination
generated: true
---

```
array<T> →
zip-cycle(
    array: array
) → array<tuple<T, U>>
```

Combines values from two arrays into two-element tuples until the longer array is exhausted, cycling each non-empty shorter array from its beginning. Returns an empty array when both inputs are empty and `null` when exactly one input is empty or either value cannot be evaluated as an array.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values form the second element of each tuple. |



## Value shape

- Pipeline input: `array<T>`
- Returns: `array<tuple<T, U>>`
- `array`: Returns `array<U>`.
- Nullability: The result is nullable when the pipeline input or the `array` parameter is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `expanded` <span class="semantics-info" title="One visited input can produce multiple output elements." aria-label="Cardinality definition: One visited input can produce multiple output elements.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`array`:** Evaluated once in the enclosing context.


## Behavior

Both inputs are materialized once. When both arrays are non-empty, the result has the cardinality of the longer array and indexes each input cyclically. Two empty inputs produce an empty array; exactly one empty input produces `null` because no value is available to cycle.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4} | zip-cycle({"a", "b"}) → {T(1, "a"), T(2, "b"), T(3, "a"), T(4, "b")}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
