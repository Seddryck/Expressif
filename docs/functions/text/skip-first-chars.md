---
layout: docs
title: "skip-first-chars"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 460
has_toc: false
permalink: /functions/text/skip-first-chars/
tags:
  - functions
  - text
generated: true
---

```
text →
skip-first-chars(
    length: integer
) → text
```

Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the length of the substring to skip. |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-skip-first-chars`
{: .member-reference }
