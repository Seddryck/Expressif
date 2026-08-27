---
layout: docs
title: "day-of-week"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 120
has_toc: false
permalink: /functions/temporal/day-of-week/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
day-of-week() → integer
```

returns a numeric value representing the day of the week (1 being Monday and 7 being Sunday) of the date passed as the argument

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | day-of-week → 1
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-day-of-week`
{: .member-reference }
