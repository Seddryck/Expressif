---
layout: docs
title: "screaming-snake-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 130
has_toc: false
permalink: /functions/text/casing/screaming-snake-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
screaming-snake-case() → text
```

Returns the input text in SCREAMING_SNAKE_CASE, uppercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | screaming-snake-case → "HELLO_WORLD"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-screaming-snake-case`
{: .member-reference }
