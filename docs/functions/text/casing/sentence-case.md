---
layout: docs
title: "sentence-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 150
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

Returns the input text in sentence case by capitalizing the first ordinary word and lowercasing subsequent ordinary words. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `AT&T`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"hello World" | sentence-case → "Hello world"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-sentence-case`
{: .member-reference }
