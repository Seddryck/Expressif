---
layout: docs
title: "add"
parent: "Arithmetic functions"
grand_parent: "Numeric functions"
nav_order: 20
has_toc: false
permalink: /functions/numeric/arithmetic/add/
tags:
  - functions
  - numeric/arithmetic
generated: true
---

```
numeric →
add(
    value: numeric,
    times?: integer
) → numeric
```

Returns the sum of the input value and the parameter value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `numeric` | Yes | The value to add to the input value. |
| `times` | `integer` | No | Number of times the addition is applied. |





## Examples

{% raw %}
```expressif
10 | add(5)      → 15
10 | add(5, 2)   → 20
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/arithmetic`  
**Aliases:** `numeric-to-add`
{: .member-reference }
