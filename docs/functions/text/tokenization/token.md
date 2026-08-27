---
layout: docs
title: "token"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/tokenization/token/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
token(
    index: integer,
    separator?: text
) → text
```

Returns the token at the specified index in the argument value. The index of the first token is 0, the second token is 1, and so on. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `index` | `integer` | Yes | An integer value between 0 and +Infinity, defining the position of the token to be returned. |
| `separator` | `text` | No | A character that delimits the substrings in this instance. |





## Examples

```expressif
"Hello World" | token(1) → "World"
"Hello World" | token(1, " ") → "World"
```


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** `text-to-token`
{: .member-reference }
