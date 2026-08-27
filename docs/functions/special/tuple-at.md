---
layout: docs
title: "tuple-at"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 120
has_toc: false
permalink: /functions/special/tuple-at/
tags:
  - functions
  - special
generated: true
---

```
tuple →
tuple-at(
    position: integer
) → any
```

Returns the tuple field at the specified zero-based position. Returns `null` when the input is not a tuple or the position is out of range.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | Specifies the zero-based position of the tuple field to return. |





## Examples

{% raw %}
```expressif
T(10, 20, 30) | tuple-at(1) → 20
```
{% endraw %}


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `tuple-at`
{: .member-reference }
