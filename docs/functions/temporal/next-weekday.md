---
layout: docs
title: "next-weekday"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 360
has_toc: false
permalink: /functions/temporal/next-weekday/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
next-weekday(
    weekday: weekday
) → date
```

Returns a new date value corresponding to the occurrence of the weekday, passed as a parameter, following the date passed as the argument.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weekday` | `weekday` | Yes | The day of week to compare to the argument. |





## Examples

{% raw %}
```expressif
#"2024-01-15" | next-weekday("Monday") → #"2024-01-22"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-next-weekday`
{: .member-reference }
