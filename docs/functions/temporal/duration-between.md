---
layout: docs
title: "duration-between"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 140
has_toc: false
permalink: /functions/temporal/duration-between/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
duration-between(
    previous: date | date-time | year-month
) → duration
```

Returns the signed duration between the current temporal value and a previous temporal value. Returns `null` when either value cannot be evaluated or the temporal values are incompatible.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `previous` | `date | date-time | year-month` | Yes | The previous temporal value to subtract from the current input. |





## Examples

```expressif
#"2024-01-15 12:30:00" | duration-between(#"2024-01-14 12:30:00") → #"2024-01-15 12:30:00" | duration-between(#"2024-01-14 12:30:00")
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** None
{: .member-reference }
