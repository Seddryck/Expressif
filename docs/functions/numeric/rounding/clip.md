---
layout: docs
title: "clip"
parent: "Rounding functions"
grand_parent: "Numeric functions"
nav_order: 20
has_toc: false
permalink: /functions/numeric/rounding/clip/
tags:
  - functions
  - numeric/rounding
generated: true
---

```
numeric →
clip(
    min: numeric,
    max: numeric
) → numeric
```

Returns the value of an argument number, unless it is smaller than min, in which case it returns min, or greater than max, in which case it returns max.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `min` | `numeric` | Yes | value returned in case the argument value is smaller than it. |
| `max` | `numeric` | Yes | value returned in case the argument value is greater than it. |





## Examples

```expressif
10 | clip(5, 15) → 10
```


**Kind:** Function  
**Scope:** `numeric/rounding`  
**Aliases:** `numeric-to-clip`
{: .member-reference }
