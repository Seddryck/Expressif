---
layout: docs
title: "prepend"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 70
has_toc: false
permalink: /functions/text/concatenation/prepend/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prepend(
    text: text
) → text
```

Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns the text specified as the parameter.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `text` | `text` | Yes | The text to prepend |





## Examples

```expressif
"Hello World" | prepend("Hi ") → "Hi Hello World"
```


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prepend`
{: .member-reference }
