---
layout: docs
title: "tokenize-lexical"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/tokenization/tokenize-lexical/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-lexical() → array
```

Returns lexical tokens in source order, preserving punctuation and symbols as separate tokens.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello, David!" | tokenize-lexical → {"Hello", ",", "David", "!"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-tokenize-lexical`
{: .member-reference }
