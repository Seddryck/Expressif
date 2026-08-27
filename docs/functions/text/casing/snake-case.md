---
layout: docs
title: "snake-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 150
has_toc: false
permalink: /functions/text/casing/snake-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
snake-case() → text
```

Returns the input text in snake_case, lowercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | snake-case → "hello_world"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-snake-case`
{: .member-reference }
