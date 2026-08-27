---
layout: docs
title: "greatest-common-divisor"
parent: "Arithmetic functions"
grand_parent: "Numeric functions"
nav_order: 70
has_toc: false
permalink: /functions/numeric/arithmetic/greatest-common-divisor/
tags:
  - functions
  - numeric/arithmetic
generated: true
---

```
numeric →
greatest-common-divisor(
    value: integer
) → numeric
```

Returns the greatest common divisor (GCD) of the argument integer and the parameter integer. Returns `null` if the argument is not an integer.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `integer` | Yes | The integer used to compute the greatest common divisor with the argument value. |





## Examples

{% raw %}
```expressif
10 | greatest-common-divisor(5) → 5
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/arithmetic`  
**Aliases:** `numeric-to-greatest-common-divisor`
{: .member-reference }
