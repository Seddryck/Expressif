---
layout: docs
title: "chunk-around"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/partitioning/chunk-around/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
chunk-around(
    position: integer
) → tuple
```

Separates the element at a zero-based position from the elements before and after it, returning the three parts as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | The zero-based position of the element to separate. |





## Behavior

`chunk-around` materializes the input and returns `T(before, selected, after)`. The selected value is preserved as a scalar, including when it is `null` or structured. A position is valid only when it identifies an existing element.



## Examples

{% raw %}
```expressif
{10, 20, 30, 40} | chunk-around(2) → T({10, 20}, 30, {40})
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
