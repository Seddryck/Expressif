---
layout: docs
title: "round"
parent: "Rounding functions"
grand_parent: "Numeric functions"
nav_order: 50
has_toc: false
permalink: /functions/numeric/rounding/round/
tags:
  - functions
  - numeric/rounding
generated: true
---

```
numeric →
round(
    digits: integer
) → numeric
```

Returns the value of an argument number to the specified number of fractional digits.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `digits` | `integer` | Yes | An integer between 0 and +Infinity, indicating the number of fractional digits in the return value. |





## Examples

```expressif
10 | round(2) → 10
```


**Kind:** Function  
**Scope:** `numeric/rounding`  
**Aliases:** `numeric-to-round`
{: .member-reference }
