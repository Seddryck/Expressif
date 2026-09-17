---
layout: docs
title: "compare-numeric"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/sorting/compare-numeric/
tags:
  - functions
  - sorting
generated: true
---

```
numeric →
compare-numeric(
    right: numeric
) → ordering
```

Compares two values after numeric coercion and returns their relative ordering.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `numeric` | Yes | The value compared with the input value. |





## Behavior

Both values are coerced to numeric. Returns #less, #equal, or #greater; returns #null when either value is null or cannot be coerced.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
