---
layout: docs
title: "join"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/combination/join/
tags:
  - functions
  - array/combination
generated: true
---

```
array →
join(
    right: array | grouping | dictionary,
    left-key: expression,
    right-key?: expression
) → array
```

Emits a pair for every matching left and right value, omitting left values without a match. Array right-hand values are grouped by key before lookup.



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

Visits each element of the array entering this join call in order; each matching group contributes its right values in their original order.

- **`right`:** Evaluated once in the enclosing context before visiting the left values; an array is grouped using the right-key expression or the reused left-key expression.
- **`left-key`:** Evaluated once with each left element as its context; when right-key is omitted for an array right-hand side, also evaluated once with each right element as its context while building the grouping.
- **`right-key`:** For an array right-hand side, evaluated once per right element with that element as its context while constructing the grouping; skipped for a grouping or dictionary.



## Examples

{% raw %}
```expressif
{{id := 1}, {id := 2}} | join({{id := 1, order := "A"}, {id := 1, order := "B"}}, .id) → {({id := 1} => {id := 1, order := "A"}), ({id := 1} => {id := 1, order := "B"})}
{1, 2} | join(!{(1 => "one")}, @_) → {(1 => "one")}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
