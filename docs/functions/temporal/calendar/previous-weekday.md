---
layout: docs
title: "previous-weekday"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 140
has_toc: false
permalink: /functions/temporal/calendar/previous-weekday/
tags:
  - functions
  - temporal/calendar
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



## Argument evaluation

- **`weekday`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
#"2024-01-15" | previous-weekday("Monday") → #"2024-01-08"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-previous-weekday`
{: .member-reference }
