---
layout: docs
title: "symmetric-difference"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/set/symmetric-difference/
tags:
  - functions
  - array/set
generated: true
---

```
array →
symmetric-difference(
    array: array
) → array
```

Returns the distinct values that appear in exactly one of the two arrays, listing pipeline-input exclusives first and parameter-array exclusives second while preserving order within each source. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array to compare against the pipeline input. |





## Examples

{% raw %}
```expressif
{1, 2, 3} | symmetric-difference({2, 3, 4}) → {1, 4}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `symmetric-difference`
{: .member-reference }
