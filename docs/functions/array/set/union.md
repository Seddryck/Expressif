---
layout: docs
title: "union"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/set/union/
tags:
  - functions
  - array/set
generated: true
---

```
array<T> →
union(
    array: array
) → array<union<T, U>>
```

Returns the distinct values appearing in either the pipeline input or the specified array, listing pipeline-input values first and argument-only values second while preserving order within each source. Returns `null` when the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values are combined with the pipeline input. |



## Examples

{% raw %}
```expressif
{1, 2, 3} | union({2, 3, 4}) → {1, 2, 3, 4}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<union<T, U>>`
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

**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `union`
{: .member-reference }
