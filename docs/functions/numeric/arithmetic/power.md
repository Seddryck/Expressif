---
layout: docs
title: "power"
parent: "Arithmetic functions"
grand_parent: "Numeric functions"
nav_order: 150
has_toc: false
permalink: /functions/numeric/arithmetic/power/
tags:
  - functions
  - numeric/arithmetic
generated: true
---

```
numeric →
power(
    exponent: numeric
) → numeric
```

Returns the the numeric argument value raised to the power specified by the parameter value.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `exponent` | `numeric` | Yes | The exponent to which the input value is raised. |





## Examples

{% raw %}
```expressif
4 | power(2) → 16
```
{% endraw %}

## Argument evaluation

- **`exponent`:** Evaluated once in the context surrounding this `power` call.

**Kind:** Function  
**Scope:** `numeric/arithmetic`  
**Aliases:** `numeric-to-power`
{: .member-reference }
