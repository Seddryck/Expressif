---
layout: docs
title: "first-chars"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 150
has_toc: false
permalink: /functions/text/first-chars/
tags:
  - functions
  - text
generated: true
---

```
text →
first-chars(
    length: integer
) → text
```

Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `length` | `integer` | Yes | An integer value between 0 and +Infinity, defining the length of the substring to return. |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-first-chars`
{: .member-reference }
