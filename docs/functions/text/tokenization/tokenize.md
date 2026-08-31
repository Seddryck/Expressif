---
layout: docs
title: "tokenize"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/tokenization/tokenize/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize(
    separator?: text
) → array
```

Returns all tokens in the argument value in source order. By default, tokenization uses white-space characters as delimiters. If a character is specified, that character delimits the tokens.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `separator` | `text` | No | A character that delimits the tokens in the argument value. |






## Examples

{% raw %}
```expressif
"Hello World" | tokenize → {"Hello", "World"}
"Hello,World" | tokenize(",") → {"Hello", "World"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-tokenize`
{: .member-reference }
