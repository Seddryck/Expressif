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





**Kind:** Function  
**Scope:** `temporal/conversion`  
**Aliases:** None
{: .member-reference }
