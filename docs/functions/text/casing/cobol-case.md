---
layout: docs
title: "cobol-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 40
has_toc: false
permalink: /functions/text/casing/cobol-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
cobol-case() → text
```

Returns the input text in COBOL-CASE, uppercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | cobol-case → "HELLO-WORLD"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-cobol-case`
{: .member-reference }
