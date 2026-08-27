---
layout: docs
title: "sentence-case"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 450
has_toc: false
permalink: /functions/text/sentence-case/
tags:
  - functions
  - text
generated: true
---

```
text →
sentence-case() → text
```

Returns the input text in sentence case by capitalizing the first word while preserving the remaining content. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `AT&T`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-sentence-case`, `capitalize`
{: .member-reference }
