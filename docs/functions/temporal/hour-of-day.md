---
layout: docs
title: "hour-of-day"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 220
has_toc: false
permalink: /functions/temporal/hour-of-day/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
hour-of-day() → integer
```

returns a numeric value representing the hours of the date passed as the argument

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | hour-of-day → 12
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-hour-of-day`
{: .member-reference }
