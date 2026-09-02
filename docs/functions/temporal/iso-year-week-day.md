---
layout: docs
title: "iso-year-week-day"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 270
has_toc: false
permalink: /functions/temporal/iso-year-week-day/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
iso-year-week-day() → text
```

returns a textual value at format YYYY-Www-D representing the year and week number (according to ISO 8601), and the day number (1 being Monday) of the date passed as the argument

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | iso-year-week-day → "2024-W03-1"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-iso-year-week-day`
{: .member-reference }
