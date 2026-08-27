---
layout: docs
title: "remove-chars"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 370
has_toc: false
permalink: /functions/text/remove-chars/
tags:
  - functions
  - text
generated: true
---

```
text →
remove-chars(
    charToRemove: text
) → text
```

Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `charToRemove` | `text` | Yes | The char to be removed from the argument string. |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-remove-chars`
{: .member-reference }
