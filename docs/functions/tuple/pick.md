---
layout: docs
title: "pick"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/tuple/pick/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
pick(
    ...positions: integer
) → tuple
```

Returns a tuple containing selected positions in the requested order.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `positions` | `integer` | Variadic (one or more) | One or more zero-based tuple positions. |






## Examples

{% raw %}
```expressif
T("John", "Smith", 42) | pick(1, 0) → T("Smith", "John")
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `pick`
{: .member-reference }
