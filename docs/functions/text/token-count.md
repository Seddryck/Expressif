---
layout: docs
title: "token-count"
parent: "Text functions"
grand_parent: "Functions library"
nav_order: 570
has_toc: false
permalink: /functions/text/token-count/
tags:
  - functions
  - text
generated: true
---

```
text →
token-count(
    separator: text
) → integer
```

Returns the count of token within the argument value. By default, the tokenization is executed based on any white-space characters. If a character is specified then the tokenization is executed based on this character to separate two tokens.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `separator` | `text` | Yes | A character that delimits the substrings in this instance. |





**Kind:** Function  
**Scope:** `text`  
**Aliases:** `text-to-token-count`
{: .member-reference }
