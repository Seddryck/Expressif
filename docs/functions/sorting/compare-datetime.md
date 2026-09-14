---
layout: docs
title: "compare-datetime"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/sorting/compare-datetime/
tags:
  - functions
  - sorting
generated: true
---

```
date-time →
compare-datetime(
    right: date-time
) → ordering
```

Compares two values after datetime coercion and returns their relative ordering.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `date-time` | Yes | The value compared with the input value. |





## Behavior

Both values are coerced to datetime. Returns #less, #equal, or #greater; returns #null when either value is null or cannot be coerced.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
