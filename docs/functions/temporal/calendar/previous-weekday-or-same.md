---
layout: docs
title: "previous-weekday-or-same"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 150
has_toc: false
permalink: /functions/temporal/calendar/previous-weekday-or-same/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
previous-weekday-or-same(
    weekday: weekday
) → date
```

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weekday` | `weekday` | Yes | The day of week to compare to the argument. |



## Argument evaluation

- **`weekday`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
#"2024-01-15" | previous-weekday-or-same("Monday") → #"2024-01-15"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-previous-weekday-or-same`
{: .member-reference }
