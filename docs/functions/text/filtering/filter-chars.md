---
layout: docs
title: "filter-chars"
parent: "Filtering functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/filtering/filter-chars/
tags:
  - functions
  - text/filtering
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





## Examples

```expressif
#null → #null
```


**Kind:** Function  
**Scope:** `text/filtering`  
**Aliases:** `text-to-filter-chars`
{: .member-reference }
