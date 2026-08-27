---
layout: docs
title: "last-of-year"
parent: "Calendar functions"
grand_parent: "Temporal functions"
nav_order: 70
has_toc: false
permalink: /functions/temporal/calendar/last-of-year/
tags:
  - functions
  - temporal/calendar
generated: true
---

```
date-time →
last-of-year() → date-time
```

Returns the 31st of December of the same year than the argument dateTime.

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | last-of-year → #"2024-12-31 00:00:00"
```


**Kind:** Function  
**Scope:** `temporal/calendar`  
**Aliases:** `dateTime-to-last-of-year`
{: .member-reference }
