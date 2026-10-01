---
layout: docs
title: "first-elements"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/selection/first-elements/
tags:
  - functions
  - array/selection
generated: true
---

```
array<T> →
first-elements(
    count: integer
) → array<T>
```

Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to return from the start of the input. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<T>`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `prefix` <span class="semantics-info" title="An output at a position depends on the visited prefix ending at that position." aria-label="Dependency definition: An output at a position depends on the visited prefix ending at that position.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`count`:** Evaluated once in the enclosing context.


## Behavior

When `count` is greater than the number of elements in the input, `first-elements` returns all available elements in their original order. It does not pad the result to reach the requested count.



## Examples

{% raw %}
```expressif
{1, 2, 3} | first-elements(2) → {1, 2}
{1, 2, 3} | first-elements(5) → {1, 2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `first`
{: .member-reference }
