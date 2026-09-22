---
layout: docs
title: "next-weekday"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 110
has_toc: false
permalink: /functions/temporal/calendar/next-weekday/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
next-weekday(
    weekday: weekday
) → date
```

Returns a new date value corresponding to the occurrence of the weekday, passed as a parameter, following the date passed as the argument.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weekday` | `weekday` | Yes | The day of week to compare to the argument. |



## Argument evaluation

- **`weekday`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
#"2024-01-15" | next-weekday("Monday") → #"2024-01-22"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-next-weekday`
{: .member-reference }
