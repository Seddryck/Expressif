---
layout: docs
title: "last-elements"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 90
has_toc: false
permalink: /functions/array/last-elements/
tags:
  - functions
  - array
generated: true
---

```
array →
last-elements(
    count: integer
) → array
```

Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to return from the end of the input. |





## Examples

```expressif
{1, 2, 3} | last-elements(2) → {2, 3}
```


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `last`
{: .member-reference }
