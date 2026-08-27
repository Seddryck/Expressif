---
layout: docs
title: "text-to-datetime"
parent: "Conversion functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/conversion/text-to-datetime/
tags:
  - functions
  - text/conversion
generated: true
---

```
text →
text-to-datetime(
    format: text,
    culture?: text
) → date-time
```

Returns a dateTime value matching the argument value parsed by the long format in the culture specified in parameter.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `format` | `text` | Yes | A string representing the required format. |
| `culture` | `text` | No | A string representing a pre-defined culture. |





**Kind:** Function  
**Scope:** `text/conversion`  
**Aliases:** `text-to-dateTime`
{: .member-reference }
