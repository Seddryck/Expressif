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

Splits an array on a zero-based boundary and returns the elements before and from that position as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | The zero-based boundary position; the element at this position belongs to the right chunk. |





## Behavior

`chunk-on` materializes the input and returns `T(before, from-position)`. Positions from zero through the input cardinality are valid, so either chunk may be empty.



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
