---
layout: docs
title: "skip-last-elements"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 80
has_toc: false
permalink: /functions/array/selection/skip-last-elements/
tags:
  - functions
  - array/selection
generated: true
---

```
array<T> →
skip-last-elements(
    count: integer
) → array<T>
```

Omits the requested number of elements from the end of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to omit from the end of the input. |



## Examples

{% raw %}
```expressif
{1, 2, 3} | skip-last-elements(2) → {1}
{1, 2, 3} | skip-last-elements(5) → {}
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

- **`count`:** Evaluated once in the enclosing context.


## Behavior

When `count` is greater than the number of elements in the input, `skip-last-elements` omits all available elements and returns an empty array. Additional requested skips have no effect.

**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `skip-last`
{: .member-reference }
