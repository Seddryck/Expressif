---
layout: docs
title: "namespace-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 90
has_toc: false
permalink: /functions/text/casing/namespace-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
namespace-case() → text
```

Returns the input text in namespace::case, lowercasing words and joining them with double colons. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | namespace-case → "hello::world"
```


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-namespace-case`
{: .member-reference }
