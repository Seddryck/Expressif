---
layout: docs
title: "slice-elements"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 90
has_toc: false
permalink: /functions/array/selection/slice-elements/
tags:
  - functions
  - array/selection
generated: true
---

```
array<T> →
slice-elements(
    start: integer,
    end: integer
) → array<T>
```

Returns the elements in the zero-based half-open range from start, inclusive, to end, exclusive. Returns `null` when the input is not an enumerable, is a string, or either bound is negative.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `start` | `integer` | Yes | Zero-based index of the first element to return. |
| `end` | `integer` | Yes | Zero-based exclusive index at which to stop returning elements. |



## Examples

{% raw %}
```expressif
{1, 2, 3} | slice-elements(1, 3) → {2, 3}
{1, 2, 3} | slice-elements(2, 1) → {}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<T>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`start`:** Evaluated once in the enclosing context.
- **`end`:** Evaluated once in the enclosing context.


## Behavior

When `start` is greater than `end`, the requested range contains no elements and `slice-elements` returns an empty array.

**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `slice`
{: .member-reference }
