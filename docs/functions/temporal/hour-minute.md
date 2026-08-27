---
layout: docs
title: "hour-minute"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 190
has_toc: false
permalink: /functions/temporal/hour-minute/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
hour-minute() → text
```

returns a textual value at format hh:mm (24 hours format) representing the hours and minutes of the dateTime passed as the argument

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | hour-minute → "12:30"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-hour-minute`
{: .member-reference }
