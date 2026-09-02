---
layout: docs
title: "second-of-day"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 460
has_toc: false
permalink: /functions/temporal/second-of-day/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
second-of-day() → integer
```

returns a numeric value representing the seconds of the day of the date passed as the argument

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | second-of-day → 45000
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-second-of-day`
{: .member-reference }
