---
layout: docs
title: "filter-chars"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 140
has_toc: false
permalink: /functions/text/filter-chars/
tags:
  - functions
  - text
generated: true
---

```
text →
filter-chars(
    filter: array | text
) → text
```

Returns only those characters specified in the parameter, in the order, they were originally entered in the input value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `filter` | `array | text` | Yes | The chars to filter from the argument string. |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-filter-chars`
{: .member-reference }
