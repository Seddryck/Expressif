---
layout: docs
title: "iso-year-week"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 250
has_toc: false
permalink: /functions/temporal/iso-year-week/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
iso-year-week() → text
```

returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | iso-year-week → "2024-W03"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-iso-year-week`
{: .member-reference }
