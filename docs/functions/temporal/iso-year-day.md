---
layout: docs
title: "iso-year-day"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 240
has_toc: false
permalink: /functions/temporal/iso-year-day/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
iso-year-day() → text
```

returns a textual value at format YYYY-ddd representing the year, and the day number of the date passed as the argument (both according to ISO 8601)

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | iso-year-day → "2024-015"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-iso-year-day`
{: .member-reference }
