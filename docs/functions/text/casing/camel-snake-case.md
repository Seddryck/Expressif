---
layout: docs
title: "camel-snake-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/casing/camel-snake-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
camel-snake-case() → text
```

Returns the input text in camel_Snake case, lowercasing the first word, capitalizing subsequent words, and joining them with underscores. Preserves `null`, empty, and blank inputs; returns `null` for a zero-length array.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | camel-snake-case → "hello_World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-camel-snake-case`
{: .member-reference }
