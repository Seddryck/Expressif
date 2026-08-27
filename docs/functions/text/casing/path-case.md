---
layout: docs
title: "path-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 120
has_toc: false
permalink: /functions/text/casing/path-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
path-case() → text
```

Returns the input text in path/case, lowercasing words and joining them with slashes. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | path-case → "hello/world"
```


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-path-case`
{: .member-reference }
