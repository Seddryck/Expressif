---
layout: docs
title: "last-in-month"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 50
has_toc: false
permalink: /functions/temporal/calendar/last-in-month/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
last-in-month(
    weekday: weekday
) → date
```

Returns a new dateTime value corresponding to the last occurrence of the weekday passed as a parameter of the month of the date passed as the argument.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weekday` | `weekday` | Yes | The day of week to compare to the argument. |





## Examples

```expressif
#"2024-01-15" | last-in-month("Monday") → #"2024-01-29"
```


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-last-in-month`
{: .member-reference }
