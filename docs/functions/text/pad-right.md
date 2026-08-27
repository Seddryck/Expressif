---
layout: docs
title: "pad-right"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 270
has_toc: false
permalink: /functions/text/pad-right/
tags:
  - functions
  - text
generated: true
---

```
text →
pad-right(
    length: integer,
    character: text
) → text
```

Returns a new string that left-aligns the characters in this string by padding them on the right with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the minimal length of the string returned |
| `character` | `text` | Yes | The padding character |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-pad-right`
{: .member-reference }
