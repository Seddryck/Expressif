---
layout: docs
title: "map"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/array/map/
tags:
  - functions
  - array
generated: true
---

```
array<T> →
map(
    transformation: expression
) → array<U>
```

Applies a transformation expression to each input item and returns the transformed values. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `transformation` | `expression` | Yes | Expression creating the transformation applied to each input item. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<U>`
- **`transformation`:** Receives `T` and returns `U`.
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array or each pair of the dictionary supplied as pipeline input to this map call, in enumeration order.

- **`transformation`:** Evaluated once per visited element, with that element as its context. For dictionary input, $key and $value resolve to the key and value of the visited pair, rather than a surrounding record.


## Behavior

Dictionaries participate directly as ordered collections of first-class pairs. Keys and values are preserved without coercion. The result is an ordinary array, and an empty dictionary produces an empty array. Records require pairs to expose their fields as pairs.



## Examples

{% raw %}
```expressif
{1, 2, 3} | map(add(1)) → {2, 3, 4}
!{"BE" => 100, "FR" => 80} | map(tuple($key, $value)) → {T("BE", 100), T("FR", 80)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `map`
{: .member-reference }
