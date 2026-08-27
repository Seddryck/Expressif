---
layout: docs
title: "pascal-snake-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 110
has_toc: false
permalink: /functions/text/casing/pascal-snake-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
pascal-snake-case() → text
```

Returns the input text in Pascal_Snake case, capitalizing each word and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | pascal-snake-case → "Hello_World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-pascal-snake-case`
{: .member-reference }
