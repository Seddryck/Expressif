---
layout: docs
title: "change-of-year"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 90
has_toc: false
permalink: /functions/temporal/change-of-year/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
change-of-year() → date-time
```

returns a temporal value corresponding to the same day and month of the argument value but of the year passed as the parameter. If the original date was the 29th of February and the year passed as a parameter is not a leap year then it returns the 28th of February.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:45" | change-of-year(2025) → #"2025-01-15 12:30:45"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-change-of-year`
{: .member-reference }
