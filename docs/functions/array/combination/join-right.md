---
layout: docs
title: "join-right"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/combination/join-right/
tags:
  - functions
  - array/combination
generated: true
---

```
array →
join-right(
    right: array | grouping | dictionary,
    left-key: expression,
    right-key?: expression
) → array
```

Emits every matching pair and preserves unmatched right values with #null in the absent side. Array right-hand values are grouped by key before lookup. Null keys match null keys; composite keys use structural equality and duplicate values produce Cartesian combinations. Null or invalid collection inputs return null.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `array | grouping | dictionary` | Yes | The array, grouping, or dictionary supplying matching right-hand values. |
| `left-key` | `expression` | Yes | Selects the lookup key of each left value. |
| `right-key` | `expression` | No | Selects the key of each right array value; when omitted, the left-key expression is reused. It is unnecessary for a grouping or dictionary. |



## Structural semantics

- **Cardinality:** `unknown`
- **Dependency:** `whole-input`
- **Ordering:** `preserved`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

## Argument evaluation

Visits each element of the array supplied as pipeline input to this join-right call in order; matching right values retain their order within the key. Preserved unmatched left values occupy their input position; preserved unmatched right values are appended in original array order or keyed entry order, with group values in order. Empty groups supply no right values.

- **`right`:** Evaluated once in the enclosing context before visiting the left values; an array is grouped using the right-key expression or the reused left-key expression.
- **`left-key`:** Evaluated once with each element of the array supplied as pipeline input to this call as its context; .field reads that element. When right-key is omitted for a right array, also evaluated once with each right element as its context before visiting left elements.
- **`right-key`:** For an array right-hand side, evaluated once per right element with that element as its context while constructing the grouping; skipped for a grouping or dictionary.



## Examples

{% raw %}
```expressif
{1, 2} | join-right({2, 3}, @_) → {(2 => 2), (#null => 3)}
```
{% endraw %}


**Kind:** Function<br>
**Scope:** `array/combination`<br>
**Aliases:** None
{: .member-reference }
