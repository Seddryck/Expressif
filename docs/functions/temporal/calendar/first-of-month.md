---
layout: docs
title: "first-of-month"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 30
has_toc: false
permalink: /functions/temporal/calendar/first-of-month/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
first-of-month() → date-time
```

Returns the first day of the month of the same month/year than the argument dateTime.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | first-of-month → #"2024-01-01 00:00:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-first-of-month`
{: .member-reference }
