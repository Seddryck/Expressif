---
layout: docs
title: "invalid-to-date"
parent: "Conversion functions"
grand_parent: "Temporal functions"
nav_order: 20
has_toc: false
permalink: /functions/temporal/conversion/invalid-to-date/
tags:
  - functions
  - temporal/conversion
generated: true
---

```
date-time →
invalid-to-date(
    default: date-time
) → date-time
```

Returns the dateTime argument except if the value is not a valid dateTime then it returns the parameter value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `default` | `date-time` | Yes | The dateTime to be returned if the argument is not a valid dateTime. |





**Kind:** Function  
**Scope:** `temporal/conversion`  
**Aliases:** None
{: .member-reference }
