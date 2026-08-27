---
layout: docs
title: "array"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/array/array/
tags:
  - functions
  - array
generated: true
---

```
any →
array(
    ...values: any
) → array
```

Constructs a new array by evaluating zero or more positional expressions from left to right against the same input. Spread arguments expand array values in place. This is the runtime-expression counterpart of array literal syntax.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `values` | `any` | Variadic (zero or more) | Zero or more expressions whose evaluated values become the elements of the resulting array. |





## Examples

{% raw %}
```expressif
#null | array → {}
#null | array(1, 2, 3) → {1, 2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `array`
{: .member-reference }
