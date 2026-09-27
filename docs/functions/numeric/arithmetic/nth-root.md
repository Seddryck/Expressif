---
layout: docs
title: "nth-root"
parent: "Arithmetic functions"
grand_parent: "Numeric functions"
nav_order: 120
has_toc: false
permalink: /functions/numeric/arithmetic/nth-root/
tags:
  - functions
  - numeric/arithmetic
generated: true
---

```
numeric →
nth-root(
    exponent: numeric
) → numeric
```

Returns the root specified by the parameter value of the numeric argument value.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `exponent` | `numeric` | Yes | The exponent of the root to return. |





## Argument evaluation

- **`exponent`:** Evaluated once in the context surrounding this `nth-root` call.



## Examples

{% raw %}
```expressif
16 | nth-root(2) → 4
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/arithmetic`  
**Aliases:** `numeric-to-nth-root`
{: .member-reference }
