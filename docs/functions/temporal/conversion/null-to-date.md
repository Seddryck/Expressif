---
layout: docs
title: "null-to-date"
parent: "Conversion functions"
grand_parent: "Temporal functions"
nav_order: 30
has_toc: false
permalink: /functions/temporal/conversion/null-to-date/
tags:
  - functions
  - temporal/conversion
generated: true
---

```
date-time →
null-to-date(
    default: date-time
) → date-time
```

Returns the dateTime argument except if the value is `null` then it returns the parameter value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `default` | `date-time` | Yes | The dateTime to be returned if the argument is `null`. |






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | null-to-date(#"2024-01-01 00:00:00") → #"2024-01-15 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/conversion`  
**Aliases:** None
{: .member-reference }
