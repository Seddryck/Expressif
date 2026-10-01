---
layout: docs
title: "last-elements"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/selection/last-elements/
tags:
  - functions
  - array/selection
generated: true
---

```
array<T> →
last-elements(
    count: integer
) → array<T>
```

Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to return from the end of the input. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<T>`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`count`:** Evaluated once in the enclosing context.


## Behavior

When `count` is greater than the number of elements in the input, `last-elements` returns all available elements in their original order. It does not pad the result to reach the requested count.



## Examples

{% raw %}
```expressif
{1, 2, 3} | last-elements(2) → {2, 3}
{1, 2, 3} | last-elements(5) → {1, 2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `last`
{: .member-reference }
