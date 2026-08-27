---
layout: docs
title: "mask-to-text"
parent: "Masking functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/masking/mask-to-text/
tags:
  - functions
  - text/masking
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





## Examples

```expressif
"Hello World" | mask-to-text("000-000") → "(null)"
```


**Kind:** Function  
**Scope:** `text/masking`  
**Aliases:** None
{: .member-reference }
