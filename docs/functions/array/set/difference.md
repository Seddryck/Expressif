---
layout: docs
title: "difference"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/set/difference/
tags:
  - functions
  - array/set
generated: true
---

```
array →
difference(
    array: array
) → array
```

Returns the distinct values from the pipeline input that do not appear in the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the array containing values to exclude from the pipeline input. |





## Examples

```expressif
{1, 2, 3} | difference({2, 3, 4}) → {1}
```


**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `difference`
{: .member-reference }
