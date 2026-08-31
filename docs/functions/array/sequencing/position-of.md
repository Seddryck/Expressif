---
layout: docs
title: "position-of"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/sequencing/position-of/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array →
position-of(
    value: any
) → integer
```

Returns the zero-based position of the first input item equal to the specified value. Returns `null` when no item matches or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `any` | Yes | Specifies the value to locate. |






## Examples

{% raw %}
```expressif
{"a", "b", "c"} | position-of("b") → 1
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `position-of`
{: .member-reference }
