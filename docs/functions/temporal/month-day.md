---
layout: docs
title: "month-day"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 320
has_toc: false
permalink: /functions/temporal/month-day/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
month-day() → text
```

returns a textual value at format MM-DD representing the month and day of the date passed as the argument

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | month-day → "01-15"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-month-day`
{: .member-reference }
