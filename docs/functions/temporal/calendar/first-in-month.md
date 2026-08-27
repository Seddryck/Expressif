---
layout: docs
title: "first-in-month"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 20
has_toc: false
permalink: /functions/temporal/calendar/first-in-month/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
first-in-month(
    weekday: weekday
) → date
```

Returns a new date value corresponding to the first occurrence of the weekday passed as a parameter of the month of the date passed as the argument.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weekday` | `weekday` | Yes | The day of week to compare to the argument. |





## Examples

```expressif
#"2024-01-15" | first-in-month("Monday") → #"2024-01-01"
```


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-first-in-month`
{: .member-reference }
