---
layout: docs
title: "before-substring"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/text/before-substring/
tags:
  - functions
  - text
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





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-before-substring`
{: .member-reference }
