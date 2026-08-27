---
layout: docs
title: "year"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 520
has_toc: false
permalink: /functions/temporal/year/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
year() → text
```

returns a textual value at format YYYY representing the year of the date passed as the argument

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | year → "2024"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-year`
{: .member-reference }
