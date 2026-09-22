---
layout: docs
title: "only"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 110
has_toc: false
permalink: /accumulators/array/only/
tags:
  - accumulators
  - array
generated: true
---

```
array →
only(
    predicate: predicate,
    accumulator: accumulator
) → any
```

Forwards only items satisfying the predicate to the wrapped accumulator.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicate` | `predicate` | Yes | Specifies the predicate deciding which items participate. |
| `accumulator` | `accumulator` | Yes | Specifies the accumulator receiving matching items. |



## Structural semantics

- **Cardinality:** `collapsed`
- **Dependency:** `whole-input`
- **Ordering:** `not-applicable`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

## Argument evaluation

Visits each item of the collection supplied as pipeline input to this only call in source order.

- **`predicate`:** Evaluated once per visited item, with that item as its context. Direct field selectors such as .active read that item; arguments within the predicate follow their normal evaluation rules.
- **`accumulator`:** Created once before accumulation, retaining the surrounding evaluation context. Receives only matching items; its arguments follow the wrapped accumulator evaluation rules.


## Behavior

The input and final result types are inherited from the wrapped accumulator. Initialization, empty-input and no-match results, completion, and failures are unchanged. Null items are evaluated by the predicate like any other item. Predicate results must be Boolean. Matching items retain source order without materializing a filtered collection. Direct pipeline calls implicitly fold the wrapper.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4} | only(is-even, count) → 2
{10, #null, 30} | fold(only(is-not-null, count)) → 2
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
