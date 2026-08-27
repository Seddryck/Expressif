---
layout: docs
title: "title-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 170
has_toc: false
permalink: /functions/text/casing/title-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
title-case() → text
```

Returns the input text in title case, capitalizing words while keeping small words lowercase only when they are neither first nor last and do not follow a colon. The first and last words are always capitalized, and a small word after a colon is capitalized. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `Q&A`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | title-case → "Hello World"
```


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-title-case`
{: .member-reference }
