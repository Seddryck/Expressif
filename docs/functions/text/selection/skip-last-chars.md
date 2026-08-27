---
layout: docs
title: "skip-last-chars"
parent: "Selection functions"
grand_parent: "Text functions"
nav_order: 60
has_toc: false
permalink: /functions/text/selection/skip-last-chars/
tags:
  - functions
  - text/selection
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





## Examples

```expressif
"Hello World" | skip-last-chars(2) → "Hello Wor"
```


**Kind:** Function  
**Scope:** `text/selection`  
**Aliases:** `text-to-skip-last-chars`
{: .member-reference }
