---
layout: docs
title: "union"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/set/union/
tags:
  - functions
  - array/set
generated: true
---

```
array →
union(
    array: array
) → array
```

Returns the distinct values appearing in either the pipeline input or the specified array, listing pipeline-input values first and argument-only values second while preserving order within each source. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values are combined with the pipeline input. |





**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `union`
{: .member-reference }
