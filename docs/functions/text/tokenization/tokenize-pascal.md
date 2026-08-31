---
layout: docs
title: "tokenize-pascal"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 80
has_toc: false
permalink: /functions/text/tokenization/tokenize-pascal/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-pascal() → array
```

Returns tokens from a PascalCase name using case and acronym transitions as boundaries.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"HTTPServerURL" | tokenize-pascal → {"HTTP", "Server", "URL"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-tokenize-pascal`
{: .member-reference }
