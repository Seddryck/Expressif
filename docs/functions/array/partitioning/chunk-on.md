---
layout: docs
title: "chunk-on"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/partitioning/chunk-on/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
chunk-on(
    position: integer
) → tuple
```

Splits an array on a zero-based boundary and returns the elements before and from that position as a tuple. Positions beyond the end use the end boundary. Returns `null` when the position is negative or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | The zero-based boundary position; the element at this position belongs to the right chunk. |





## Behavior

`chunk-on` materializes the input and returns `T(before, from-position)`. Positions beyond the input cardinality use the end boundary, so the right chunk is empty. Negative positions return `null`.



## Examples

{% raw %}
```expressif
{10, 20, 30, 40} | chunk-on(2) → T({10, 20}, {30, 40})
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
