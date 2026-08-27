---
layout: docs
title: "complement"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/set/complement/
tags:
  - functions
  - array/set
generated: true
---

```
array →
complement(
    array: array
) → array
```

Returns the distinct values from the specified array that do not appear in the pipeline input, preserving the specified array order. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the reference array from which values present in the pipeline input are excluded. |





## Examples

```expressif
{1, 2, 3} | complement({2, 3, 4}) → {4}
```


**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `complement`
{: .member-reference }
