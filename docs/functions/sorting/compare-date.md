---
layout: docs
title: "compare-date"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/sorting/compare-date/
tags:
  - functions
  - sorting
generated: true
---

```
date →
compare-date(
    right: date
) → ordering
```

Compares two values after date coercion and returns their relative ordering.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `date` | Yes | The value compared with the input value. |





## Behavior

Both values are coerced to date. Returns #less, #equal, or #greater; returns #null when either value is null or cannot be coerced.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
