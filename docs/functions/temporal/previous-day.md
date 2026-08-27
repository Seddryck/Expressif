---
layout: docs
title: "previous-day"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 400
has_toc: false
permalink: /functions/temporal/previous-day/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
previous-day() → date-time
```

Returns the dateTime that substract a day to the dateTime passed as argument value.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | previous-day → #"2024-01-14 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-previous-day`
{: .member-reference }
