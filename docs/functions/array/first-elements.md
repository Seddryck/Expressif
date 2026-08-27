---
layout: docs
title: "first-elements"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/array/first-elements/
tags:
  - functions
  - array
generated: true
---

```
array →
first-elements(
    count: integer
) → array
```

Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Number of elements to return from the start of the input. |





**Kind:** Function  
**Scope:** `array`  
**Aliases:** `first`
{: .member-reference }
