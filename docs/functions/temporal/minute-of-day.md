---
layout: docs
title: "minute-of-day"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 290
has_toc: false
permalink: /functions/temporal/minute-of-day/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
minute-of-day() → integer
```

returns a numeric value representing the minutes of the date passed as the argument

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | minute-of-day → 750
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-minute-of-day`
{: .member-reference }
