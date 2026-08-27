---
layout: docs
title: "after-substring"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/text/after-substring/
tags:
  - functions
  - text
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





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-after-substring`
{: .member-reference }
