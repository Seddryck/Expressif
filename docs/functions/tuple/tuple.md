---
layout: docs
title: "tuple"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/tuple/tuple/
tags:
  - functions
  - tuple
generated: true
---

```
any →
tuple(
    ...values: any
) → tuple
```

Constructs a new tuple by evaluating zero or more positional expressions from left to right against the same input. Spread arguments expand array values in place.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `values` | `any` | Variadic (zero or more) | Zero or more expressions whose evaluated values become the positions of the resulting tuple. |






## Examples

{% raw %}
```expressif
#null | tuple() → T()
#null | tuple(1, "a", #true) → T(1, "a", #true)
#null | tuple(...{1, 2, 3}, 4) → T(1, 2, 3, 4)
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `tuple`
{: .member-reference }
