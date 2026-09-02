---
layout: docs
title: "iso-week-of-year"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 240
has_toc: false
permalink: /functions/temporal/iso-week-of-year/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
iso-week-of-year() → integer
```

returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | iso-week-of-year → 3
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-iso-week-of-year`
{: .member-reference }
