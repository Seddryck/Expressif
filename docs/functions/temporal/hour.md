---
layout: docs
title: "hour"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 180
has_toc: false
permalink: /functions/temporal/hour/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
hour() → text
```

returns a textual value at format hh (24 hours format) representing the hours of the dateTime passed as the argument

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | hour → "12"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-hour`
{: .member-reference }
