---
layout: docs
title: "only"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 140
has_toc: false
permalink: /functions/array/aggregation/only/
tags:
  - functions
  - array/aggregation
generated: true
---

```
T →
only(
    predicate: predicate,
    accumulator: accumulator
) → U
```

Forwards only items satisfying the predicate to the wrapped accumulator.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicate` | `predicate` | Yes | Specifies the predicate deciding which items participate. |
| `accumulator` | `accumulator` | Yes | Specifies the accumulator receiving matching items. |



## Examples

{% raw %}
```expressif
{1, 2, 3, 4} | only(is-even, count) → 2
{10, #null, 30} | fold(only(is-not-null, count)) → 2
```
{% endraw %}

## Value shape

- Pipeline input: `T`
- Returns: `U`
- `predicate`: Receives `T` and returns `boolean`.
- `accumulator`: Receives `T` and returns `U`.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.




## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.


## Argument evaluation

Visits each item of the collection supplied as pipeline input to this only call in source order.

- **`predicate`:** Evaluated once per visited item, with that item as its context. Direct field selectors such as .active read that item; arguments within the predicate follow their normal evaluation rules.
- **`accumulator`:** Created once before accumulation, retaining the surrounding evaluation context. Receives only matching items; its arguments follow the wrapped accumulator evaluation rules.


## Behavior

The input and final result types are inherited from the wrapped accumulator. Initialization, empty-input and no-match results, completion, and failures are unchanged. Null items are evaluated by the predicate like any other item. Predicate results must be Boolean. Matching items retain source order without materializing a filtered collection. Direct pipeline calls implicitly fold the wrapper.

**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
