---
layout: docs
title: "percent-change"
parent: "Arithmetic functions"
grand_parent: "Numeric functions"
nav_order: 140
has_toc: false
permalink: /functions/numeric/arithmetic/percent-change/
tags:
  - functions
  - numeric/arithmetic
generated: true
---

```
numeric →
percent-change(
    previous: numeric
) → numeric
```

Returns the percentage change from the previous numeric value to the current input value. Returns `null` when the input or parameter cannot be evaluated or when the previous value is zero.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `previous` | `numeric` | Yes | Specifies the previous numeric value used as the percentage-change baseline. |





## Examples

{% raw %}
```expressif
10 | percent-change(8) → 25.00
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/arithmetic`  
**Aliases:** `numeric-to-percent-change`
{: .member-reference }
