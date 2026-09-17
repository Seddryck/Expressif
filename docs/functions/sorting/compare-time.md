---
layout: docs
title: "compare-time"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/sorting/compare-time/
tags:
  - functions
  - sorting
generated: true
---

```
time →
compare-time(
    right: time
) → ordering
```

Compares two values after time coercion and returns their relative ordering.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `time` | Yes | The value compared with the input value. |





## Behavior

Both values are coerced to time. Returns #less, #equal, or #greater; returns #null when either value is null or cannot be coerced.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
