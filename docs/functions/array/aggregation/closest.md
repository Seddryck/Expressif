---
layout: docs
title: "closest"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/aggregation/closest/
tags:
  - functions
  - array/aggregation
generated: true
---

```
T →
closest(
    target: any
) → nullable<T>
```

Returns the first non-null input value with the smallest absolute distance to the target.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `target` | `any` | Yes | Specifies the reference value used to measure numeric or temporal distance. |



## Value shape

- **Pipeline input:** `T`
- **Returns:** `nullable<T>`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.




## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.


## Argument evaluation

- **`target`:** Evaluated once before accumulation, using the incoming collection as its context. The result is reused for every comparison.


## Behavior

The result preserves the selected input value and its type. Numeric targets use subtract; other targets use duration-between. Compatibility and coercion follow those operations. Values whose distance is null are ignored. Ties preserve input order. A null target, empty input, or input without a valid distance returns null. Arithmetic overflow follows the underlying difference operation.



## Examples

{% raw %}
```expressif
{10, 30, 50} | closest(32) → 30
{30, 10} | closest(20) → 30
{#null, 10, 30} | closest(20) → 10
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
