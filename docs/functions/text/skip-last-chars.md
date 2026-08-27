---
layout: docs
title: "skip-last-chars"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 470
has_toc: false
permalink: /functions/text/skip-last-chars/
tags:
  - functions
  - text
generated: true
---

```
text →
skip-last-chars(
    length: integer
) → text
```

Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the length of the substring to skip. |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-skip-last-chars`
{: .member-reference }
