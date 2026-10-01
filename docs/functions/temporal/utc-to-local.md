---
layout: docs
title: "utc-to-local"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 460
has_toc: false
permalink: /functions/temporal/utc-to-local/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
utc-to-local(
    timeZoneLabel: text
) → date-time
```

Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `timeZoneLabel` | `text` | Yes | The time-zone identifier or display-name label to convert to. |





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | utc-to-local("UTC") → #"2024-01-15 12:30:00"
```
{% endraw %}

## Argument evaluation

- **`timeZoneLabel`:** Evaluated once in the context surrounding this `utc-to-local` call.

**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** None
{: .member-reference }
