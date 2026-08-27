---
layout: docs
title: "previous-month"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 410
has_toc: false
permalink: /functions/temporal/previous-month/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
previous-month() → date-time
```

Returns the dateTime that substract a month to the dateTime passed as argument value.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | previous-month → #"2023-12-15 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-previous-month`
{: .member-reference }
