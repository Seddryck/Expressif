---
layout: docs
title: "zip-strict"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/combination/zip-strict/
tags:
  - functions
  - array/combination
generated: true
---

```
array →
zip-strict(
    array: array
) → array
```

Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the equally sized second array whose values form the second element of each tuple. |






## Examples

{% raw %}
```expressif
{1, 2} | zip-strict({"a", "b"}) → {T(1, "a"), T(2, "b")}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
