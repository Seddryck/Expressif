---
layout: docs
title: "circular-distance"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/temporal/circular-distance/
tags:
  - functions
  - temporal
generated: true
---

```
time →
circular-distance(
    reference: time
) → duration
```

Returns the shortest unsigned duration between the current time and a reference time on a 24-hour clock. Returns `null` when either time is `null`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `time` | Yes | The time from which to measure the shortest distance around the clock. |






## Examples

{% raw %}
```expressif
#"23:45:00" | circular-distance(#"01:00:00") → #"2024-01-01T01:15:00" | duration-between(#"2024-01-01T00:00:00")
#"16:00:00" | circular-distance(#"23:45:00") → #"2024-01-01T07:45:00" | duration-between(#"2024-01-01T00:00:00")
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** None
{: .member-reference }
