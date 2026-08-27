---
layout: docs
title: "forward"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 170
has_toc: false
permalink: /functions/temporal/forward/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
forward(
    time: time,
    times?: integer
) → date-time
```

Returns a dateTime that adds the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `time` | `time` | Yes | The value to be added to the argument value |
| `times` | `integer` | No | An integer between 0 and +Infinity, indicating the number of times to repeat the addition |





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | forward("01:30:00") → #"2024-01-15 14:00:00"
#"2024-01-15 12:30:00" | forward("01:30:00", 2) → #"2024-01-15 15:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-forward`, `dateTime-to-add`
{: .member-reference }
