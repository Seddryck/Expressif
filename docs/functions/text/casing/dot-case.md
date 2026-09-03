---
layout: docs
title: "dot-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/casing/dot-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
dot-case() → text
```

Returns the input text in dot.case, lowercasing words and joining them with periods. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | dot-case → "hello.world"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-dot-case`
{: .member-reference }
