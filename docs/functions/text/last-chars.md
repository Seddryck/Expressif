---
layout: docs
title: "last-chars"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 190
has_toc: false
permalink: /functions/text/last-chars/
tags:
  - functions
  - text
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





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-last-chars`
{: .member-reference }
