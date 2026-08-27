---
layout: docs
title: "swap-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 160
has_toc: false
permalink: /functions/text/casing/swap-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
swap-case() → text
```

Returns the input text with lowercase characters converted to uppercase and uppercase characters converted to lowercase. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | swap-case → "hELLO wORLD"
```


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-swap-case`
{: .member-reference }
