---
layout: docs
title: "divide"
parent: "Arithmetic functions"
grand_parent: "Numeric functions"
nav_order: 60
has_toc: false
permalink: /functions/numeric/arithmetic/divide/
tags:
  - functions
  - numeric/arithmetic
generated: true
---

```
numeric →
divide(
    value: numeric
) → numeric
```

Returns the argument number divided by the parameter value. If the parameter value is `0`, it returns `null`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `numeric` | Yes | The value to divide the argument value. |





## Examples

{% raw %}
```expressif
10 | divide(5) → 2
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/arithmetic`  
**Aliases:** `numeric-to-divide`
{: .member-reference }
