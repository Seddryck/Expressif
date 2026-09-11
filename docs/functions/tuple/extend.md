---
layout: docs
title: "extend"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/tuple/extend/
tags:
  - functions
  - tuple
generated: true
---

```
tuple | vector →
extend(
    value: any
) → tuple | vector
```

Returns a new tuple with a value appended, expanding tuple values into their positions.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `any` | Yes | Specifies the value to append; tuple values are expanded into their positions. |



## Argument evaluation

- **`value`:** Evaluated once against the value entering this call.



## Examples

{% raw %}
```expressif
T(1, 2) | extend(3) → T(1, 2, 3)
T(1, 2) | extend(T(3, "foo")) → T(1, 2, 3, "foo")
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `extend`
{: .member-reference }
