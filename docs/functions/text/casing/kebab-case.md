---
layout: docs
title: "kebab-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 70
has_toc: false
permalink: /functions/text/casing/kebab-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
kebab-case() → text
```

Returns the input text in kebab-case, lowercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | kebab-case → "hello-world"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-kebab-case`
{: .member-reference }
