---
layout: docs
title: "lower"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 80
has_toc: false
permalink: /functions/text/casing/lower/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
lower() → text
```

Returns the input text converted to lowercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | lower → "hello world"
```


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-lower`
{: .member-reference }
