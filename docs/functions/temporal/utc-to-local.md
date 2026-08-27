---
layout: docs
title: "utc-to-local"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 510
has_toc: false
permalink: /functions/temporal/utc-to-local/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
utc-to-local() → date-time
```

Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter.

## Parameters



This function has no parameters.





## Examples

```expressif
#"2024-01-15 12:30:00" | utc-to-local("UTC") → #"2024-01-15 12:30:00"
```


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** None
{: .member-reference }
