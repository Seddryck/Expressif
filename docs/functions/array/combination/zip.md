---
layout: docs
title: "zip"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/combination/zip/
tags:
  - functions
  - array/combination
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
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
