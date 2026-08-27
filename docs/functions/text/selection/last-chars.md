---
layout: docs
title: "last-chars"
parent: "Selection functions"
grand_parent: "Text functions"
nav_order: 40
has_toc: false
permalink: /functions/text/selection/last-chars/
tags:
  - functions
  - text/selection
generated: true
---

```
text →
last-chars(
    length: integer
) → text
```

Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the length of the substring to return. |





## Examples

```expressif
"Hello World" | last-chars(2) → "ld"
```


**Kind:** Function  
**Scope:** `text/selection`  
**Aliases:** `text-to-last-chars`
{: .member-reference }
