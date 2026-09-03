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

Returns the input text in namespace::case, lowercasing words and joining them with double colons. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | namespace-case → "hello::world"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-namespace-case`
{: .member-reference }
