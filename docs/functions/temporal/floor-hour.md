---
layout: docs
title: "floor-hour"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 160
has_toc: false
permalink: /functions/temporal/floor-hour/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
floor-hour() → date-time
```

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | floor-hour → #"2024-01-15 12:00:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-floor-hour`
{: .member-reference }
