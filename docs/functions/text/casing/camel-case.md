---
layout: docs
title: "camel-case"
parent: "Casing functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/casing/camel-case/
tags:
  - functions
  - text/casing
generated: true
---

```
text →
camel-case() → text
```

Returns the input text in camelCase, lowercasing the first word and capitalizing subsequent words without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | camel-case → "helloWorld"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/casing`  
**Aliases:** `text-to-camel-case`
{: .member-reference }
