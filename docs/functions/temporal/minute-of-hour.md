---
layout: docs
title: "minute-of-hour"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 290
has_toc: false
permalink: /functions/temporal/minute-of-hour/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
minute-of-hour() → integer
```

returns a numeric value representing the minutes of the hour passed as the argument

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | minute-of-hour → 30
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-minute-of-hour`
{: .member-reference }
