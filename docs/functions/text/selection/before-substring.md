---
layout: docs
title: "before-substring"
parent: "Selection functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/selection/before-substring/
tags:
  - functions
  - text/selection
generated: true
---

```
text →
before-substring(
    substring: text,
    count?: integer
) → text
```

Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `substring` | `text` | Yes | The string to seek. |
| `count` | `integer` | No | The number of character positions to examine. |





## Examples

```expressif
"Hello World" | before-substring("lo") → "Hel"
"Hello World" | before-substring("lo", 2) → "(null)"
```


**Kind:** Function  
**Scope:** `text/selection`  
**Aliases:** `text-to-before-substring`
{: .member-reference }
