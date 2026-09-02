---
layout: docs
title: "floor-minute"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 170
has_toc: false
permalink: /functions/temporal/floor-minute/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
floor-minute() → date-time
```

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | floor-minute → #"2024-01-15 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-floor-minute`
{: .member-reference }
