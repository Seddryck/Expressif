---
layout: docs
title: "length-of-month"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 80
has_toc: false
permalink: /functions/temporal/calendar/length-of-month/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
length-of-month() → integer
```

returns the count of days within the month of the dateTime value passed as the argument. If the argument is not a dateTime but a text at format "YYYY-MM", it returns count of days of the month represented by this value. It returns a value between 28 and 31 (depending of leap year and month).

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | length-of-month → 31
```


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-length-of-month`
{: .member-reference }
