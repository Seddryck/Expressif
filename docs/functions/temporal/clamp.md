---
layout: docs
title: "clamp"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/temporal/clamp/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
clamp(
    min: date-time,
    max: date-time
) → date-time
```

Returns the value of an argument dateTime, unless it is before min (in which case it returns min), or after max (in which case it returns max).

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `min` | `date-time` | Yes | value returned in case the argument value is before than it |
| `max` | `date-time` | Yes | value returned in case the argument value is after than it |





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | clamp(#"2024-01-01 00:00:00", #"2024-01-31 00:00:00") → #"2024-01-15 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-clamp`, `dateTime-to-clip`
{: .member-reference }
