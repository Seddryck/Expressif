---
layout: docs
title: "pascal-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 100
has_toc: false
permalink: /functions/text/casing/pascal-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
pascal-case() → text
```

Returns the input text in PascalCase, capitalizing each word and removing separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | pascal-case → "HelloWorld"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-pascal-case`
{: .member-reference }
