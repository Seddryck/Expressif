---
layout: docs
title: "zip-padded"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 70
has_toc: false
permalink: /functions/array/combination/zip-padded/
tags:
  - functions
  - array/combination
generated: true
---

```
array<T> →
zip-padded(
    array: array
) → array<tuple<nullable<T>, nullable<U>>>
```

Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values form the second element of each tuple. |



## Examples

{% raw %}
```expressif
{1, 2, 3} | zip-padded({"a", "b"}) → {T(1, "a"), T(2, "b"), T(3, #null)}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<tuple<nullable<T>, nullable<U>>>`
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
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
