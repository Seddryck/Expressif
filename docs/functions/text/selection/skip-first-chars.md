---
layout: docs
title: "skip-first-chars"
parent: "Selection functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/selection/skip-first-chars/
tags:
  - functions
  - text/selection
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





## Examples

```expressif
"Hello World" | skip-first-chars(2) → "llo World"
```


**Kind:** Function  
**Scope:** `text/selection`  
**Aliases:** `text-to-skip-first-chars`
{: .member-reference }
