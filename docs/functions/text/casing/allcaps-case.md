---
layout: docs
title: "allcaps-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/casing/allcaps-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
allcaps-case() → text
```

Returns the input text in ALLCAPS case, uppercasing words and concatenating them without separators. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | allcaps-case → "HELLOWORLD"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-allcaps-case`
{: .member-reference }
