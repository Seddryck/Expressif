---
layout: docs
title: "zip"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 190
has_toc: false
permalink: /functions/array/zip/
tags:
  - functions
  - array
generated: true
---

```
array →
zip(
    array: array
) → array
```

Combines corresponding values from the input array and a second array into two-element tuples, stopping when either array is exhausted. Returns `null` when either value cannot be evaluated as an array.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values form the second element of each tuple. |






## Examples

{% raw %}
```expressif
{1, 2, 3} | zip({"a", "b"}) → {T(1, "a"), T(2, "b")}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
