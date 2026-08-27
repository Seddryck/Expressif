---
layout: docs
title: "token"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 560
has_toc: false
permalink: /functions/text/token/
tags:
  - functions
  - text
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





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-token`
{: .member-reference }
