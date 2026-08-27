---
layout: docs
title: "change-of-month"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/temporal/change-of-month/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
change-of-month() → date-time
```

returns a temporal value corresponding to the same day and year of the argument value but of the month passed as the parameter. If the original day is 29, 30, or 31 and the new month passed as a parameter has fewer days then it returns the last day of the corresponding month.

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:45" | change-of-month(6) → #"2024-06-15 12:30:45"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-change-of-month`
{: .member-reference }
