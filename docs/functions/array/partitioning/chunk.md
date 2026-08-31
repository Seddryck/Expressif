---
layout: docs
title: "chunk"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/partitioning/chunk/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
chunk(
    size: integer
) → array
```

Splits an array into consecutive, non-overlapping chunks of at most the specified size, preserving a final partial chunk. It resembles a count-based tumbling window but, unlike general sliding or hopping windows, has no separate step and always keeps the final partial chunk. It does not group items by inactivity or time. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `size` | `integer` | Yes | The strictly positive number of items in each chunk. |






## Examples

{% raw %}
```expressif
{1, 2, 3} | chunk(2) → {{1, 2}, {3}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** `chunk`
{: .member-reference }
