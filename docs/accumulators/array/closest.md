---
layout: docs
title: "closest"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 20
has_toc: false
permalink: /accumulators/array/closest/
tags:
  - accumulators
  - array
generated: true
---

```
any →
closest(
    target: any
) → any
```

Returns the first non-null input value with the smallest absolute distance to the target.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `target` | `any` | Yes | Specifies the reference value used to measure numeric or temporal distance. |

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


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
