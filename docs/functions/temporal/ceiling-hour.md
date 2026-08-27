---
layout: docs
title: "ceiling-hour"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/temporal/ceiling-hour/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
ceiling-hour() → date-time
```

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero and an hour added.

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | ceiling-hour → #"2024-01-15 13:00:00"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-ceiling-hour`
{: .member-reference }
