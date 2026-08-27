---
layout: docs
title: "pad-left"
parent: "Padding functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/padding/pad-left/
tags:
  - functions
  - text/padding
generated: true
---

```
text →
pad-left(
    length: integer,
    character: text
) → text
```

Returns a new string that right-aligns the characters in this string by padding them on the left with a specified character, for a specified total length. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the minimal length of the string returned |
| `character` | `text` | Yes | The padding character |





## Examples

```expressif
"Hello World" | pad-left(2, "-") → "Hello World"
```


**Kind:** Function  
**Scope:** `text/padding`  
**Aliases:** `text-to-pad-left`
{: .member-reference }
