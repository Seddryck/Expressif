---
layout: docs
title: "upper"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 190
has_toc: false
permalink: /functions/text/casing/upper/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
upper() → text
```

Returns the input text converted to uppercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | upper → "HELLO WORLD"
```


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-upper`
{: .member-reference }
