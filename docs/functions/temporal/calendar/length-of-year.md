---
layout: docs
title: "length-of-year"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 90
has_toc: false
permalink: /functions/temporal/calendar/length-of-year/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
length-of-year() → integer
```

Returns the count of days within the year of the dateTime value passed as the argument. If the argument is not a dateTime but an integer, returns count of days of the corresponding year. It returns 365 or 366 (for leap years).

## Parameters



This function has no parameters.





**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-length-of-year`
{: .member-reference }
