---
layout: docs
title: "backward"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/temporal/backward/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
backward(
    time: time,
    times?: integer
) → date-time
```

Returns a dateTime that subtract the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `time` | `time` | Yes | The value to be subtracted to the argument value. |
| `times` | `integer` | No | An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction |






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | backward("01:30:00") → #"2024-01-15 11:00:00"
#"2024-01-15 12:30:00" | backward("01:30:00", 2) → #"2024-01-15 09:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-backward`, `dateTime-to-subtract`
{: .member-reference }
