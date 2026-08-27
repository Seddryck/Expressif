---
layout: docs
title: "mask-to-text"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 220
has_toc: false
permalink: /functions/text/mask-to-text/
tags:
  - functions
  - text
generated: true
---

```
text →
mask-to-text(
    mask: text
) → text
```

Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `mask` | `text` | Yes | The string representing the mask to be unset from the argument string. |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
