---
layout: docs
title: "distribute-weight"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 80
has_toc: false
permalink: /functions/array/partitioning/distribute-weight/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
distribute-weight(
    weight: expression
) → array
```

Distributes array values into two groups whose aggregate evaluated weights are approximately balanced. Returns `null` when the input or a weight cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weight` | `expression` | Yes | Specifies the expression that produces a finite, non-negative numeric weight for each input value. |





## Behavior

`distribute-weight` evaluates `weight` exactly once per input value, orders values by descending weight using original position to break equal-weight ties, and assigns each value to the group with the lower aggregate weight. Aggregate-weight ties prefer the group with fewer values; remaining ties prefer the first group. It then restores relative input order within both groups. This deterministic largest-weight-first strategy is best-effort and does not guarantee the mathematically optimal partition. Empty input returns two empty arrays and a singleton is placed in the first group.



## Examples

{% raw %}
```expressif
{8, 7, 6, 5} | distribute-weight(neutral) → {{8, 5}, {7, 6}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
