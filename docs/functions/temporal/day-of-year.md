---
layout: docs
title: "day-of-year"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 130
has_toc: false
permalink: /functions/temporal/day-of-year/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
day-of-year() → integer
```

returns a numeric value representing the day position within the year of the date passed as the argument

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | day-of-year → 15
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-day-of-year`
{: .member-reference }
