---
layout: docs
title: "first-of-year"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 40
has_toc: false
permalink: /functions/temporal/calendar/first-of-year/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
first-of-year() → date-time
```

Returns the first of January of the same year than the argument dateTime.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | first-of-year → #"2024-01-01 00:00:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-first-of-year`
{: .member-reference }
