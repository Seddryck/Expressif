---
layout: docs
title: "text-to-mask"
parent: "Masking functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/masking/text-to-mask/
tags:
  - functions
  - text/masking
generated: true
---

```
text →
text-to-mask(
    mask: text
) → text
```

Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `mask` | `text` | Yes | The string representing the mask to apply to the argument string. |





## Examples

```expressif
"Hello World" | text-to-mask("000-000") → "000-000"
```


**Kind:** Function  
**Scope:** `text/masking`  
**Aliases:** None
{: .member-reference }
