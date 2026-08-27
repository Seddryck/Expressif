---
layout: docs
title: "sentence-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 140
has_toc: false
permalink: /functions/text/casing/sentence-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
sentence-case() → text
```

Returns the input text in sentence case by capitalizing the first word while preserving the remaining content. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `AT&T`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | sentence-case → "Hello World"
```


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-sentence-case`, `capitalize`
{: .member-reference }
