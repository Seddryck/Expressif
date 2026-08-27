---
layout: docs
title: "intersection"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/set/intersection/
tags:
  - functions
  - array/set
generated: true
---

```
array →
intersection(
    array: array
) → array
```

Returns the distinct values found in both the pipeline input and the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the array to compare with the pipeline input. |





**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `intersection`
{: .member-reference }
