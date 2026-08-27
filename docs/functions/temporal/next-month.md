---
layout: docs
title: "next-month"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 350
has_toc: false
permalink: /functions/temporal/next-month/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
next-month() → date-time
```

Returns the dateTime that adds a month to the dateTime passed as argument value.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | next-month → #"2024-02-15 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-next-month`
{: .member-reference }
