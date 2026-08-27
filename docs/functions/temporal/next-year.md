---
layout: docs
title: "next-year"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 380
has_toc: false
permalink: /functions/temporal/next-year/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
next-year() → date-time
```

Returns the dateTime that adds a year to the dateTime passed as argument value.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | next-year → #"2025-01-15 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-next-year`
{: .member-reference }
