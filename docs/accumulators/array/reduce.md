---
layout: docs
title: "reduce"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 110
has_toc: false
permalink: /accumulators/array/reduce/
tags:
  - accumulators
  - array
generated: true
---

```
array →
reduce(
    operation: expression,
    initial?: any
) → any
```

Combines array elements in source order by repeatedly evaluating an expression against the accumulated value and current element.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the expression evaluated against each accumulated-value/current-element tuple. |
| `initial` | `any` | No | Specifies the initial accumulated value and the result returned for an empty array. |



## Argument evaluation

Visits the array supplied to this reduce call in source order, starting with the second element when initial is omitted and with the first when initial is supplied.

- **`operation`:** Evaluated once per visited element with T(accumulator, current item) as input and argument context; $0 is the accumulated value and $1 the current item. Tuple-valued items remain nested in their position.
- **`initial`:** Evaluated once before accumulation, using the incoming collection as its context.


## Behavior

The combining expression receives a two-element tuple: `$0` is the accumulated value and `$1` is the current element. Without `initial`, the first element becomes the accumulated value and an empty array returns `null`. With `initial`, evaluation starts by combining it with the first element, and an empty array returns the initial value unchanged. `f~` invokes accumulator | f(current item); `~f` invokes current item | f(accumulator). Existing complete expressions and the normalization of an explicitly supplied leading $0 are preserved and are not deprecated.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4} | reduce(add($0, $1)) → 10
{1, 2, 3} | reduce(add($0, $1), 10) → 16
{20, 3, 2} | reduce(subtract~) → 15
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `reduce`
{: .member-reference }
