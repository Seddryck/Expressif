---
layout: docs
title: "ceiling-minute"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/temporal/ceiling-minute/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
ceiling-minute() → date-time
```

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero and a minute added.

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | ceiling-minute → #"2024-01-15 12:30:00"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-ceiling-minute`
{: .member-reference }
