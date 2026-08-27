---
layout: docs
title: "after-substring"
parent: "Selection functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/selection/after-substring/
tags:
  - functions
  - text/selection
generated: true
---

```
text →
after-substring(
    substring: text,
    count?: integer
) → text
```

Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `substring` | `text` | Yes | The string to seek. |
| `count` | `integer` | No | The number of character positions to examine. |





## Examples

```expressif
"Hello World" | after-substring("lo") → " World"
"Hello World" | after-substring("lo", 2) → "(null)"
```


**Kind:** Function  
**Scope:** `text/selection`  
**Aliases:** `text-to-after-substring`
{: .member-reference }
