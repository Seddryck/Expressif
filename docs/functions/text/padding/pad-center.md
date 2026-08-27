---
layout: docs
title: "pad-center"
parent: "Padding functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/padding/pad-center/
tags:
  - functions
  - text/padding
generated: true
---

```
text →
pad-center(
    length: integer,
    character: text
) → text
```

Returns a new string that center-aligns the characters in this string by padding them on both the left and the right with a specified character, for a specified total length. If the padding cannot be symetrical then the padding char is added on the right. If the length of the argument value is longer than the parameter value then the argument value is returned unmodified.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the minimal length of the string returned |
| `character` | `text` | Yes | The padding character |





## Examples

```expressif
"Hello World" | pad-center(2, "-") → "Hello World"
```


**Kind:** Function  
**Scope:** `text/padding`  
**Aliases:** `text-to-pad-center`
{: .member-reference }
