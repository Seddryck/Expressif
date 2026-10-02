---
layout: docs
title: "local-to-utc"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 280
has_toc: false
permalink: /functions/temporal/local-to-utc/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
local-to-utc(
    timeZoneLabel: text
) → date-time
```

Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `timeZoneLabel` | `text` | Yes | The time-zone identifier or display-name label of the input value. |





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | local-to-utc("UTC") → #"2024-01-15 12:30:00"
```
{% endraw %}

## Argument evaluation

- **`timeZoneLabel`:** Evaluated once in the context surrounding this `local-to-utc` call.

**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** None
{: .member-reference }
