---
layout: docs
title: "value-at"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/selection/value-at/
tags:
  - functions
  - array/selection
generated: true
---

```
array →
value-at(
    position: integer
) → any
```

Returns the input item at the specified zero-based position. Returns `null` when the position is negative or out of range, or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | Specifies the zero-based position of the item to return. |






## Examples

{% raw %}
```expressif
{"a", "b", "c"} | value-at(1) → "b"
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** `value-at`
{: .member-reference }
