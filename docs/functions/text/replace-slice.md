---
layout: docs
title: "replace-slice"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 390
has_toc: false
permalink: /functions/text/replace-slice/
tags:
  - functions
  - text
generated: true
---

```
text →
replace-slice(
    start: integer,
    length: integer,
    append: text
) → text
```

Returns the argument value with a subset of the string substitued by a another string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `start` | `integer` | Yes | The position to start to replace |
| `length` | `integer` | Yes | The length to replace |
| `append` | `text` | Yes | The text to append when the slice has been removed |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-replace-slice`
{: .member-reference }
