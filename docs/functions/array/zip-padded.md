---
layout: docs
title: "zip-padded"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 220
has_toc: false
permalink: /functions/array/zip-padded/
tags:
  - functions
  - array
generated: true
---

```
array →
zip-padded(
    array: array
) → array
```

Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values form the second element of each tuple. |






## Examples

{% raw %}
```expressif
{1, 2, 3} | zip-padded({"a", "b"}) → {T(1, "a"), T(2, "b"), T(3, #null)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
