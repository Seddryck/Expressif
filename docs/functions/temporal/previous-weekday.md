---
layout: docs
title: "previous-weekday"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 420
has_toc: false
permalink: /functions/temporal/previous-weekday/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
previous-weekday(
    weekday: weekday
) → date
```

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weekday` | `weekday` | Yes | The day of week to compare to the argument. |





## Examples

```expressif
#"2024-01-15" | previous-weekday("Monday") → #"2024-01-08"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-previous-weekday`
{: .member-reference }
