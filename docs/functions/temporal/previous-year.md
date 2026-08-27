---
layout: docs
title: "previous-year"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 440
has_toc: false
permalink: /functions/temporal/previous-year/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
previous-year() → date-time
```

Returns the dateTime that substract a year to the dateTime passed as argument value.

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | previous-year → #"2023-01-15 12:30:00"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-previous-year`
{: .member-reference }
