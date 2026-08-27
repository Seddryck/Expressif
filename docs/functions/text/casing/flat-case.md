---
layout: docs
title: "flat-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 60
has_toc: false
permalink: /functions/text/casing/flat-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
flat-case() → text
```

Returns the input text in flatcase, lowercasing words and concatenating them without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | flat-case → "helloworld"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-flat-case`
{: .member-reference }
