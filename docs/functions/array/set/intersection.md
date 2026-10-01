---
layout: docs
title: "intersection"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/set/intersection/
tags:
  - functions
  - array/set
generated: true
---

```
array<T> →
intersection(
    array: array
) → array<T>
```

Returns the distinct values found in both the pipeline input and the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the array to compare with the pipeline input. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<T>`
- **Nullability:** The result is nullable when the pipeline input or the `array` parameter is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`array`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
{1, 2, 3} | intersection({2, 3, 4}) → {2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `intersection`
{: .member-reference }
