---
layout: docs
title: "zip-strict"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 80
has_toc: false
permalink: /functions/array/combination/zip-strict/
tags:
  - functions
  - array/combination
generated: true
---

```
array<T> →
zip-strict(
    array: array
) → array<tuple<T, U>>
```

Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the equally sized second array whose values form the second element of each tuple. |



## Value shape

- Pipeline input: `array<T>`
- Returns: `array<tuple<T, U>>`
- `array`: Returns `array<U>`.
- Nullability: The result is nullable when the pipeline input or the `array` parameter is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`array`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
{1, 2} | zip-strict({"a", "b"}) → {T(1, "a"), T(2, "b")}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
