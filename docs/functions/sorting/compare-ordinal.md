---
layout: docs
title: "compare-ordinal"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/sorting/compare-ordinal/
tags:
  - functions
  - sorting
generated: true
---

```
text →
compare-ordinal(
    right: text
) → ordering
```

Compares two values as text using deterministic ordinal ordering.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `text` | Yes | The value compared with the input value. |





## Behavior

Both values are coerced to text and compared with ordinal, non-locale-sensitive semantics. Returns #less, #equal, or #greater; returns #null when either value is null or cannot be coerced.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
