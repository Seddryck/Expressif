---
layout: docs
title: "set-time"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 480
has_toc: false
permalink: /functions/temporal/set-time/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
set-time(
    instant: text
) → date-time
```

Returns a dateTime with the time part set to the value passed as parameter and the date part corresponding to the argument value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `instant` | `text` | Yes | The time value to set as hours, minutes, seconds of the dateTime argument |





## Examples

```expressif
#"2024-01-15 12:30:00" | set-time("01:30:00") → #"2024-01-15 01:30:00"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-set-time`
{: .member-reference }
