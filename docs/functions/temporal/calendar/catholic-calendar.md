---
layout: docs
title: "catholic-calendar"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 10
has_toc: false
permalink: /functions/temporal/calendar/catholic-calendar/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
catholic-calendar() → date-time
```

Returns the date of the Catholic calendar event passed as parameter for the year specified by the argument. Returns `null` if the event is unknown.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
2024 | catholic-calendar("Easter Sunday") → #"2024-03-31 00:00:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `calendar-catholic`
{: .member-reference }
